using DonationService.Models;
using Microsoft.EntityFrameworkCore;

namespace DonationService.Data
{
    public class DonationDBContext:DbContext
    {
        public DonationDBContext(DbContextOptions<DonationDBContext> options)
            : base(options)
        {
        }
        public DbSet<DonationRequest>  DonationRequests => Set<DonationRequest>();
    }
}
