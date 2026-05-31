using LibrarySQLBackend.Models;

namespace LibrarySQLBackend.Repositories.Interfaces
{
    public interface IFineRepository
    {
        Task<IEnumerable<Fine>> GetAllAsync();
        Task<Fine?> GetByIdAsync(int id);
        Task<IEnumerable<Fine>> GetByLoanerIdAsync(int loanerId);
        Task<IEnumerable<Fine>> GetByLoanIdAsync(int loanId);
        Task<Fine> AddAsync(Fine fine);
        Task<bool> UpdateAsync(Fine fine);
        Task<bool> DeleteAsync(int id);
    }
}