using LibrarySQLBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories.Interfaces
{
    public interface ILoanRepository
    {
        Task<Loan?> GetByIdAsync(int id);
        Task<int> CreateLoanAsync(int loanerId, int inventoryId);
        Task ReturnLoanAsync(int loanId);
    }
}
