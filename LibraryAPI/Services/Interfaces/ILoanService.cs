using LibraryAPI.DTOs;

namespace LibraryAPI.Services.Interfaces
{
    public interface ILoanService
    {
        Task<LoanDto?> GetByIdAsync(int id);
        Task<LoanDto> CreateLoanAsync(CreateLoanDto dto);
        Task ReturnLoanAsync(int loanId);
    }
}
