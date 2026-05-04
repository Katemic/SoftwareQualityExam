namespace LibraryAPI.DTOs
{
    public class CreateItemDto
    {
        public string? Name { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Description { get; set; }
        public string? MediaType { get; set; }
        public string? Image { get; set; }

        public int? LanguageId { get; set; }
        public int? PublisherId { get; set; }

        public BookDto? Book { get; set; }
        public BoardgameDto? Boardgame { get; set; }

        public List<int> CreatorIds { get; set; } = new();
        public List<int> GenreIds { get; set; } = new();
        public List<int> TagIds { get; set; } = new();
    }
}
