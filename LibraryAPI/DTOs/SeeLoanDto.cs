namespace LibraryAPI.DTOs
{
    public class SeeLoanDto
    {
        public int Id { get; set; }

        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string? Status { get; set; }

        public int LoanerId { get; set; }
        public int InventoryId { get; set; }

        public string? InventoryStatus { get; set; }
        public string? Barcode { get; set; }
        public string? Placement { get; set; }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? MediaType { get; set; }
        public int? ReleaseYear { get; set; }
    }
}
