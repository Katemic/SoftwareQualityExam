using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibrarySQLBackend.Repositories.Interfaces;

namespace LibrarySQLBackend.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Inventory?> GetByIdAsync(int inventoryId)
        {
            return await _context.Inventories
                .FirstOrDefaultAsync(i => i.Id == inventoryId);
        }

        public async Task UpdateAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Inventory>> GetAllByItemIdAsync(int itemId)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Where(i => i.ItemId == itemId)
                .OrderBy(i => i.Id)
                .ToListAsync();
        }

    }
}
