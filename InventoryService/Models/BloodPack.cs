namespace InventoryService.Models
{
    public class BloodPack
    {
        public Guid Id { get; set; }
        public Guid BloodId { get; set; }
        public Guid DonorId { get; set; }
        public DateOnly CollectionDate { get; set; }
        public DateOnly ExpirationDate { get; set; }
    }
}
