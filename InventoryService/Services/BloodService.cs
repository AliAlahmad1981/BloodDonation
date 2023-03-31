using Grpc.Core;
using InventoryService;
using InventoryService.Data;
using InventoryService.Models;

namespace InventoryService.Services
{
    public class BloodInventoryService : BloodInventory.BloodInventoryBase
    {
        private readonly ILogger<BloodInventoryService> _logger;
        private readonly InventoryDBContext _inventoryDBContext;

        public BloodInventoryService(ILogger<BloodInventoryService> logger, InventoryDBContext inventoryDBContext)
        {
            _logger = logger;
            _inventoryDBContext = inventoryDBContext;
        }

        public override async Task<AddBloodPackResponse> AddBloodPack(AddBloodPackRequest request, ServerCallContext context)
        {
            BloodPack bloodPack = new BloodPack()
            {
                Id = Guid.NewGuid(),
                BloodId = new Guid(request.BloodId),
                DonorId = new Guid(request.DonorId),
                CollectionDate = DateOnly.FromDateTime(request.CollectionDate.ToDateTime()),
                ExpirationDate = DateOnly.FromDateTime(request.ExpirationDate.ToDateTime())
            };

            await _inventoryDBContext.BloodPacks.AddAsync(bloodPack);
            await _inventoryDBContext.SaveChangesAsync();
            string stringId = bloodPack.Id.ToString();
            return new AddBloodPackResponse
            {
                Id = stringId
            };
        }
    }
}