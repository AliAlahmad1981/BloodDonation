using Grpc.Core;
using InventoryService;

namespace InventoryService.Services
{
    public class BloodInventoryService : BloodInventory.BloodInventoryBase
    {
        private readonly ILogger<BloodInventoryService> _logger;
        public BloodInventoryService(ILogger<BloodInventoryService> logger)
        {
            _logger = logger;
        }

        public override Task<AddBloodPackResponse> AddBloodPack(AddBloodPackRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AddBloodPackResponse
            {
                 Id = Guid.NewGuid().ToString()
            });
        }
    }
}