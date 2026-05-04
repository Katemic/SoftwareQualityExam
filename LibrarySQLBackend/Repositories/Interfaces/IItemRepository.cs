using LibrarySQLBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<IEnumerable<Item>> GetAllAsync();
        Task<Item?> GetByIdAsync(int id);
        Task<Item> AddAsync(Item item);
        Task<bool> UpdateAsync(Item item);
        Task<bool> DeleteAsync(int id);

        Task<List<Creator>> GetCreatorsByIdsAsync(List<int> ids);
        Task<List<Genre>> GetGenresByIdsAsync(List<int> ids);
        Task<List<Tag>> GetTagsByIdsAsync(List<int> ids);

        void RemoveBook(Book book);
        void RemoveBoardgame(Boardgame boardgame);
    }
}
