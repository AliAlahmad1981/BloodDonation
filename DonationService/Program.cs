using DonationService;
using DonationService.Data;
using DonationService.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<DonationDBContext>(options =>
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DonationDBContext>();
    dbContext.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseAuthorization();

// Add donation 
app.MapPost("/donation",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Donor,Admin")]
async (DonationRequest donation, DonationDBContext db) =>
    {
        donation.Id = Guid.NewGuid();
        await db.DonationRequests.AddAsync(donation);
        await db.SaveChangesAsync();
        return Results.Created($"/donation/{donation.Id}", "donation requested Successfully.");
    });

// Cancel donation 
app.MapPut("/donation/cancel/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Donor")]
async (string id, DonationDBContext db) =>
    {
        if (Guid.TryParse(id, out var idGuid))
        {
            var donationToUpdate = await db.DonationRequests.FindAsync(idGuid);
            if (donationToUpdate == null)
                return Results.NotFound("donation Not Found.");
            donationToUpdate.Status = DonationRequestStatus.Cancelled;
            await db.SaveChangesAsync();
            return Results.Ok("donation Updated Successfully.");
        }
        else { return Results.BadRequest("Invalid donation Id"); }
    });

// Collect donation 
app.MapPut("/donation/collect/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
async (string id, DonationDBContext db) =>
    {
        if (Guid.TryParse(id, out var idGuid))
        {
            var donationToUpdate = await db.DonationRequests.FindAsync(idGuid);
            if (donationToUpdate == null)
                return Results.NotFound("donation Not Found.");
            using var channel = GrpcChannel.ForAddress("http://inventoryservice");
            var client = new BloodInventory.BloodInventoryClient(channel);
            var result = await client.AddBloodPackAsync(new AddBloodPackRequest
            {
                BloodId = donationToUpdate.BloodId.ToString(),
                DonorId = donationToUpdate.DonorId.ToString(),
                CollectionDate = Timestamp.FromDateTimeOffset(DateTime.Now),
                ExpirationDate = Timestamp.FromDateTimeOffset(DateTime.Now.AddDays(42))

            });
            donationToUpdate.Status = DonationRequestStatus.Collected;
            await db.SaveChangesAsync();
            return Results.Ok("donation collected Successfully.");
        }
        else { return Results.BadRequest("Invalid donation Id"); }
    });

// Delete donation
app.MapDelete("/donation/{id}",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
async (string id, DonationDBContext db) =>
    {
        if (Guid.TryParse(id, out var idGuid))
        {
            var donation = await db.DonationRequests.FindAsync(idGuid);
            if (donation == null)
                return Results.NotFound("donation Not Found.");
            db.DonationRequests.Remove(donation);
            await db.SaveChangesAsync();
            return Results.Ok("donation Removed Successfully.");
        }
        else { return Results.BadRequest("Invalid donation Id"); }
    });

// Find donation
app.MapGet("/donation/{id}", async (string id, DonationDBContext db) =>
{
    if (Guid.TryParse(id, out var idGuid))
    {
        var donation = await db.DonationRequests.FindAsync(idGuid);
        return donation != null ? Results.Ok(donation) : Results.NotFound("donation Not Found.");
    }
    else { return Results.BadRequest("Invalid donation Id"); }
});

// Get donations List
app.MapGet("/donations", async (DonationDBContext db) => await db.DonationRequests.ToListAsync());


app.Run();

