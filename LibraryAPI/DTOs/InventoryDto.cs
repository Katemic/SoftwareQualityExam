namespace LibraryAPI.DTOs
{
    public class InventoryDto
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string Status { get; set; } = null!;

        public string Barcode { get; set; } = null!;

        public string? Placement { get; set; }
    }
}
