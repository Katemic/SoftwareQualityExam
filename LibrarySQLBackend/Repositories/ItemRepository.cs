using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            return await _context.Items
                .Include(i => i.Language)
                .Include(i => i.Publisher)
                .Include(i => i.Book)
                .Include(i => i.Boardgame)
                .ToListAsync();
        }

        public async Task<Item?> GetByIdAsync(int id)
        {
            return await _context.Items
                .Include(i => i.Language)
                .Include(i => i.Publisher)
                .Include(i => i.Book)
                .Include(i => i.Boardgame)
                .Include(i => i.Creators)
                .Include(i => i.Genres)
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Item> AddAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateAsync(Item item)
        {
            var existingItem = await _context.Items.FindAsync(item.Id);
            if (existingItem == null)
            {
                return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
                return false;

            _context.Items.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Creator>> GetCreatorsByIdsAsync(List<int> ids)
        {
            return await _context.Creators
                .Where(c => ids.Contains(c.Id))
                .ToListAsync();
        }

        public async Task<List<Genre>> GetGenresByIdsAsync(List<int> ids)
        {
            return await _context.Genres
                .Where(g => ids.Contains(g.Id))
                .ToListAsync();
        }

        public async Task<List<Tag>> GetTagsByIdsAsync(List<int> ids)
        {
            return await _context.Tags
                .Where(t => ids.Contains(t.Id))
                .ToListAsync();
        }

        public void RemoveBook(Book book)
        {
            _context.Books.Remove(book);
        }

        public void RemoveBoardgame(Boardgame boardgame)
        {
            _context.Boardgames.Remove(boardgame);
        }
    }
}
