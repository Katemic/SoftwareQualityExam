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

        public async Task<Reservation?> CreateReservationAsync(int loanerId, int itemId)
        {
            // Check if item exists
            var itemExists = await _context.Items
                .AnyAsync(i => i.Id == itemId);

            if (!itemExists)
            {
                return null;
            }

            // Check if loaner exists
            var loanerExists = await _context.Loaners
                .AnyAsync(l => l.Id == loanerId);

            if (!loanerExists)
            {
                return null;
            }

            // Get next queue number
            int nextQueueNumber = (await GetByItemIdAsync(itemId))?
                .Count + 1 ?? 1;

            var reservation = new Reservation
            {
                LoanerId = loanerId,
                ItemId = itemId,
                Status = "pending",
                QueueNumber = nextQueueNumber
            };

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
    }
}
