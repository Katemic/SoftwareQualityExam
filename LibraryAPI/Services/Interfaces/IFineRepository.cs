using LibraryAPI.DTOs;

namespace LibraryAPI.Services.Interfaces
{
    public interface IFineService
    {
        Task<IEnumerable<FineDto>> GetAllAsync();
        Task<FineDto?> GetByIdAsync(int id);
        Task<IEnumerable<FineDto>> GetByLoanerIdAsync(int loanerId);
        Task<FineDto> CreateAsync(CreateFineDto dto);
        Task PayFineAsync(int fineId);
        Task<bool> DeleteAsync(int id);
    }
}