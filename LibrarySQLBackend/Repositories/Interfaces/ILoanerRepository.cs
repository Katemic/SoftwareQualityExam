using LibrarySQLBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories.Interfaces
{
    public interface ILoanerRepository
    {
        Task<IEnumerable<Loaner>> GetAllAsync();
        Task<Loaner?> GetByIdAsync(int id);
        Task<Loaner?> GetByEmailAsync(string email);
        Task<Loaner> AddAsync(Loaner loaner);
        Task<bool> UpdateAsync(Loaner loaner);
        Task<bool> DeleteAsync(int id);
    }
}
