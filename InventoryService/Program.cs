using InventoryService.Data;
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<InventoryDBContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateActor = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        options.SaveToken = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddGrpc();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDBContext>();
    dbContext.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.
app.MapGrpcService<BloodInventoryService>();
// Add Blood Type
app.MapPost("/blood",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
async (Blood blood, InventoryDBContext db) =>
    {
        blood.Id = Guid.NewGuid();
        await db.Bloods.AddAsync(blood);
        await db.SaveChangesAsync();
        return Results.Created($"/blood/{blood.Id}", "Blood Added  Successfully.");
    });

// Update Blood Type
app.MapPut("/blood/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
async (string id, Blood blood, InventoryDBContext db) =>
    {
        if (Guid.TryParse(id, out var idGuid))
        {
            var bloodToUpdate = await db.Bloods.FindAsync(idGuid);
            if (bloodToUpdate == null)
                return Results.NotFound("blood Not Found.");
            bloodToUpdate.BloodType = blood.BloodType;
            bloodToUpdate.MinimumPacks = blood.MinimumPacks;
            bloodToUpdate.MaximumPacks = blood.MaximumPacks;
            await db.SaveChangesAsync();
            return Results.Ok("blood Updated Successfully.");
        }
        else { return Results.BadRequest("Invalid blood Id"); }
    });

// Delete blood
app.MapDelete("/blood/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
async (string id, InventoryDBContext db) =>
    {
        if (Guid.TryParse(id, out var idGuid))
        {
            var blood = await db.Bloods.FindAsync(idGuid);
            if (blood == null)
                return Results.NotFound("blood Not Found.");
            db.Bloods.Remove(blood);
            await db.SaveChangesAsync();
            return Results.Ok("blood Removed Successfully.");
        }
        else { return Results.BadRequest("Invalid blood Id"); }
    });

// Find blood
app.MapGet("/blood/{id}", async (string id, InventoryDBContext db) =>
{
    if (Guid.TryParse(id, out var idGuid))
    {
        var blood = await db.Bloods.FindAsync(idGuid);
        return blood != null ? Results.Ok(blood) : Results.NotFound("blood Not Found.");
    }
    else { return Results.BadRequest("Invalid blood Id"); }
});

// Get bloods List
app.MapGet("/bloods", async (InventoryDBContext db) => await db.Bloods.ToListAsync());

app.Run();

