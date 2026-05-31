namespace LibraryAPI.DTOs
{
    public class CreateFineDto
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}