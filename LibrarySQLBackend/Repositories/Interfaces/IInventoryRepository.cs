using LibrarySQLBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<Inventory?> GetByIdAsync(int inventoryId);
        Task UpdateAsync(Inventory inventory);
        Task<List<Inventory>> GetAllByItemIdAsync(int itemId);
    }
}
