namespace LibraryAPI.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public LoanerDto? User { get; set; }
    }
}
