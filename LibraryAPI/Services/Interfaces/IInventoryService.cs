using LibraryAPI.DTOs;

namespace LibraryAPI.Services.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryDto>> GetAllByItemIdAsync(int itemId);
    }
}
