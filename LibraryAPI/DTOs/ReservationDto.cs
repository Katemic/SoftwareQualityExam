using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LibraryAPI.DTOs
{
    public class ReservationDto
    {
        public int ItemId { get; set; }
        public int LoanerId { get; set; }
        public string Status { get; set; }
        public int queue_number { get; set; }
    }
}
