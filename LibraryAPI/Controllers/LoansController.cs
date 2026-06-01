using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);

            if (loan == null)
                return NotFound();

            return Ok(loan);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan(CreateLoanDto dto)
        {
            try
            {
                var loan = await _loanService.CreateLoanAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = loan.Id },
                    loan
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id)
        {
            try
            {
                await _loanService.ReturnLoanAsync(id);
                return Ok(new { message = "Loan returned successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-loans")]
        public async Task<IActionResult> GetMyLoans([FromQuery] bool includeReturned = false)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized(new { message = "User id was not found in token." });

            if (!int.TryParse(userIdClaim, out var loanerId))
                return Unauthorized(new { message = "Invalid user id in token." });

            var loans = await _loanService.GetMyLoansAsync(loanerId, includeReturned);

            return Ok(loans);
        }
    }
}

