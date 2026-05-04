namespace LibraryAPI.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ReleaseYear { get; set; }
        public string? MediaType { get; set; }
        public decimal? AverageStars { get; set; }
    }
}
