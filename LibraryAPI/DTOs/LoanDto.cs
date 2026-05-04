namespace LibraryAPI.DTOs
{
    public class LoanDto
    {
        public int Id { get; set; }
        public DateTime? LoanDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string? Status { get; set; }

        public int LoanerId { get; set; }
        public int InventoryId { get; set; }

        public string? InventoryStatus { get; set; }
    }
}
