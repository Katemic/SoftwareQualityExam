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

        public async Task<Reservation?> CreateReservationAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);

            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<List<Reservation>?> GetByItemIdAsync(int itemId)
        {
            return await _context.Reservations
            .Where(r => r.ItemId == itemId)
            .ToListAsync();
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
