using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LibrarySQLBackend.Repositories.Interfaces;

namespace LibrarySQLBackend.Repositories
{
    public class LoanerRepository : ILoanerRepository
    {
        private readonly AppDbContext _context;

        public LoanerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Loaner>> GetAllAsync()
        {
            return await _context.Loaners.ToListAsync();
        }

        public async Task<Loaner?> GetByIdAsync(int id)
        {
            return await _context.Loaners.FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Loaner?> GetByEmailAsync(string email)
        {
            return await _context.Loaners.FirstOrDefaultAsync(l => l.Email == email);
        }

        public async Task<Loaner> AddAsync(Loaner loaner)
        {
            _context.Loaners.Add(loaner);
            await _context.SaveChangesAsync();
            return loaner;
        }

        public async Task<bool> UpdateAsync(Loaner loaner)
        {
            var existing = await _context.Loaners.FindAsync(loaner.Id);
            if (existing == null)
                return false;

            _context.Entry(existing).CurrentValues.SetValues(loaner);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var loaner = await _context.Loaners.FindAsync(id);
            if (loaner == null)
                return false;

            _context.Loaners.Remove(loaner);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
