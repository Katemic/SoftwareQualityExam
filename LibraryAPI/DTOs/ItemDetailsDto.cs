using System.Text.Json.Serialization;

namespace LibraryAPI.DTOs
{
    public class ItemDetailsDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? ReviewSummary { get; set; }
        public string? MediaType { get; set; }
        public string? Image { get; set; }
        public decimal? AverageStars { get; set; }
        public string? Language { get; set; }
        public string? Publisher { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BookDto? BookDetails { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BoardgameDto? BoardgameDetails { get; set; }

        public List<string> Creators { get; set; } = new();
        public List<string> Genres { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }
}
