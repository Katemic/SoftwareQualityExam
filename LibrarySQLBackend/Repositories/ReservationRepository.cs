using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Reservation>?> GetAllAsync()
        {
            return await _context.Reservations.ToListAsync();
        }
        public async Task<Reservation?> CreateReservationAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            return reservation;
        }
        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations.FindAsync(id);
        }
        public async Task<List<Reservation>?> GetByItemIdAsync(int itemId)
        {
            return await _context.Reservations
            .Where(r => r.ItemId == itemId)
            .ToListAsync();
        }
        public async Task<List<Reservation>?> GetByLoanerId(int loanerId)
        {
            return await _context.Reservations
                .Where(r => r.LoanerId == loanerId)
                .ToListAsync();
        }
        public async Task UpdateAsync(Reservation reservation)
        {
            _context.Reservations.Update(reservation);

            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Reservation reservation)
        {
            _context.Reservations.Remove(reservation);

            await _context.SaveChangesAsync();
        }

        public async Task<bool> ItemExistsAsync(int itemId)
        {
            var itemExists = await _context.Items
                .AnyAsync(i => i.Id == itemId);

            if(!itemExists)
            {
                return false;    
            }
            return true;
        }

        public async Task<bool> LoanerExistsAsync(int loanerId)
        {
            var loanerExists = await _context.Loaners
                .AnyAsync(l => l.Id == loanerId);

            if (!loanerExists)
            {
                return false;
            }
            return true;
        }

        
    }
}
