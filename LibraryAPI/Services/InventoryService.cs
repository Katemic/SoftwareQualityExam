using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Repositories.Interfaces;

namespace LibraryAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<InventoryDto>> GetAllByItemIdAsync(int itemId)
        {
            var inventoryCopies = await _inventoryRepository.GetAllByItemIdAsync(itemId);

            return inventoryCopies.Select(i => new InventoryDto
            {
                Id = i.Id,
                ItemId = i.ItemId,
                Status = i.Status,
                Barcode = i.Barcode,
                Placement = i.Placement
            });
        }
    }
}
