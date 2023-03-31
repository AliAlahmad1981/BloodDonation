namespace InventoryService.Models
{
    public class Blood
    {
        public Guid Id { get;set; }
        public string BloodType { get; set; }
        public string MinimumPacks { get; set; }
        public string MaximumPacks { get; set; }     
    }
}
