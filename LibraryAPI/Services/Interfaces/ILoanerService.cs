using LibraryAPI.DTOs;

namespace LibraryAPI.Services.Interfaces
{
    public interface ILoanerService
    {
        Task<IEnumerable<LoanerDto>> GetAllAsync();
        Task<LoanerDto?> GetByIdAsync(int id);
        Task<LoanerDto> RegisterAsync(RegisterLoanerDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<LoanerDto?> UpdateAsync(int id, UpdateLoanerDto dto);
        Task<bool> DeleteAsync(int id);
        Task ChangePasswordAsync(int loanerId, ChangePasswordDto dto);
    }
}
