using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FinesController : ControllerBase
    {
        private readonly IFineService _fineService;

        public FinesController(IFineService fineService)
        {
            _fineService = fineService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var fines = await _fineService.GetAllAsync();
            return Ok(fines);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var fine = await _fineService.GetByIdAsync(id);

            if (fine == null)
                return NotFound();

            return Ok(fine);
        }

        [HttpGet("loaner/{loanerId}")]
        public async Task<IActionResult> GetByLoanerId(int loanerId)
        {
            var fines = await _fineService.GetByLoanerIdAsync(loanerId);
            return Ok(fines);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFineDto dto)
        {
            try
            {
                var fine = await _fineService.CreateAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = fine.Id },
                    fine
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/pay")]
        public async Task<IActionResult> PayFine(int id)
        {
            try
            {
                await _fineService.PayFineAsync(id);
                return Ok(new { message = "Fine paid successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _fineService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}