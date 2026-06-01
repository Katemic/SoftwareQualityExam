using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _context;

        public LoanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Loan?> GetByIdAsync(int id)
        {
            return await _context.Loans
                .Include(l => l.Inventory)
                .Include(l => l.Loaner)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Loan> CreateLoanAsync(Loan loan)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == loan.InventoryId);

            if (inventory == null)
                throw new InvalidOperationException("Inventory could not be found while creating the loan.");

            inventory.Status = "loaned out";

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
        INSERT INTO loan 
            (loan_date, due_date, return_date, status, loaner_id, inventory_id)
        VALUES 
            (CURRENT_TIMESTAMP(), DATE_ADD(CURRENT_TIMESTAMP(), INTERVAL 14 DAY), NULL, {loan.Status}, {loan.LoanerId}, {loan.InventoryId});
    ");

            await _context.SaveChangesAsync();

            var createdLoanId = await _context.Loans
                .Where(l =>
                    l.LoanerId == loan.LoanerId &&
                    l.InventoryId == loan.InventoryId &&
                    l.ReturnDate == null &&
                    l.Status == loan.Status)
                .OrderByDescending(l => l.Id)
                .Select(l => l.Id)
                .FirstAsync();

            return await GetByIdAsync(createdLoanId)
                   ?? throw new InvalidOperationException("Loan could not be created.");
        }

        public async Task ReturnLoanAsync(int loanId)
        {
            var loan = await _context.Loans
                .Include(l => l.Inventory)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null)
                throw new InvalidOperationException("The loan was not found while returning the loan.");

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
        UPDATE loan
        SET 
            return_date = CURRENT_TIMESTAMP(),
            status = 'returned'
        WHERE id = {loanId};
    ");

            if (loan.Inventory != null)
            {
                loan.Inventory.Status = "available";
            }

            await _context.SaveChangesAsync();

            await _context.Entry(loan).ReloadAsync();
        }

        public async Task<bool> HasUnpaidFineAsync(int loanerId)
        {
            return await _context.Fines
                .Include(f => f.Loan)
                .AnyAsync(f =>
                    f.Loan.LoanerId == loanerId &&
                    (f.Status == "unpaid" || f.Status == "late"));
        }

        public async Task<int> CountActiveLoansAsync(int loanerId)
        {
            return await _context.Loans
                .CountAsync(l =>
                    l.LoanerId == loanerId &&
                    l.ReturnDate == null);
        }

        public async Task<bool> HasOverdueLoanAsync(int loanerId)
        {
            return await _context.Loans
                .AnyAsync(l =>
                    l.LoanerId == loanerId &&
                    l.ReturnDate == null &&
                    (
                        l.Status == "overdue"
                    ));
        }


        public async Task<IEnumerable<Loan>> GetAllByLoanerIdAsync(int loanerId, bool includeReturned)
        {
            var query = _context.Loans
                .Include(l => l.Inventory)
                    .ThenInclude(i => i.Item)
                .Include(l => l.Loaner)
                .Where(l => l.LoanerId == loanerId);

            if (!includeReturned)
            {
                query = query.Where(l => l.ReturnDate == null);
            }

            return await query
                .OrderByDescending(l => l.LoanDate)
                .ToListAsync();
        }

    }
}
