using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibrarySQLBackend.Repositories
{
    public class FineRepository : IFineRepository
    {
        private readonly AppDbContext _context;

        public FineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Fine>> GetAllAsync()
        {
            return await _context.Fines
                .Include(f => f.Loan)
                .ToListAsync();
        }

        public async Task<Fine?> GetByIdAsync(int id)
        {
            return await _context.Fines
                .Include(f => f.Loan)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Fine>> GetByLoanerIdAsync(int loanerId)
        {
            return await _context.Fines
                .Include(f => f.Loan)
                .Where(f => f.Loan.LoanerId == loanerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Fine>> GetByLoanIdAsync(int loanId)
        {
            return await _context.Fines
                .Where(f => f.LoanId == loanId)
                .ToListAsync();
        }

        public async Task<Fine> AddAsync(Fine fine)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO fine
                    (amount, status, created_date, paid_date, loan_id)
                VALUES
                    ({fine.Amount}, {fine.Status}, CURRENT_TIMESTAMP(), NULL, {fine.LoanId});
            ");

            return await _context.Fines
                .Include(f => f.Loan)
                .OrderByDescending(f => f.Id)
                .FirstAsync(f =>
                    f.LoanId == fine.LoanId &&
                    f.Status == fine.Status);
        }

        public async Task<bool> UpdateAsync(Fine fine)
        {
            var existingFine = await _context.Fines.FindAsync(fine.Id);

            if (existingFine == null)
                return false;

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                UPDATE fine
                SET
                    status = {fine.Status},
                    paid_date = CURRENT_TIMESTAMP()
                WHERE id = {fine.Id};
            ");

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var fine = await _context.Fines.FindAsync(id);

            if (fine == null)
                return false;

            _context.Fines.Remove(fine);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}