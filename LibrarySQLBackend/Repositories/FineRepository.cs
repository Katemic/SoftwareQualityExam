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
            _context.Fines.Add(fine);
            await _context.SaveChangesAsync();
            return fine;
        }

        public async Task<bool> UpdateAsync(Fine fine)
        {
            var existingFine = await _context.Fines.FindAsync(fine.Id);

            if (existingFine == null)
                return false;

            _context.Entry(existingFine).CurrentValues.SetValues(fine);
            await _context.SaveChangesAsync();

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