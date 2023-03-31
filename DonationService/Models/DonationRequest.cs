namespace DonationService.Models
{
    public class DonationRequest
    {
        public Guid Id { get; set; }
        public Guid DonorId { get; set; }
        public Guid BloodId { get; set; }   
        public DonationRequestStatus Status { get; set; }
    }
}
