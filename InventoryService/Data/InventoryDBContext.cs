using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data
{
    public class InventoryDBContext : DbContext
    {
        public InventoryDBContext(DbContextOptions<InventoryDBContext> options)
            : base(options)
        {
        }
        public DbSet<Blood>  Bloods => Set<Blood>();
        public DbSet<BloodPack> BloodPacks=> Set<BloodPack>();
    }
}
