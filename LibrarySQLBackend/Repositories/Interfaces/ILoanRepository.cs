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
        Task<Loan> CreateLoanAsync(Loan loan);
        Task ReturnLoanAsync(int loanId);

        Task<bool> HasUnpaidFineAsync(int loanerId);
        Task<int> CountActiveLoansAsync(int loanerId);
        Task<bool> HasOverdueLoanAsync(int loanerId);
        Task<IEnumerable<Loan>> GetAllByLoanerIdAsync(int loanerId, bool includeReturned);
    }
}
