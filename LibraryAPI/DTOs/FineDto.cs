namespace LibraryAPI.DTOs
{
    public class FineDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public int LoanId { get; set; }
    }
}