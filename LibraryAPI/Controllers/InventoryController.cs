using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("item/{itemId}")]
        public async Task<ActionResult<IEnumerable<InventoryDto>>> GetAllByItemId(int itemId)
        {
            var inventoryCopies = await _inventoryService.GetAllByItemIdAsync(itemId);

            return Ok(inventoryCopies);
        }
    }
}
