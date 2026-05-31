using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanerController : ControllerBase
    {
        private readonly ILoanerService _loanerService;

        public LoanerController(ILoanerService loanerService)
        {
            _loanerService = loanerService;
        }
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var loaner = await _loanerService.GetByIdAsync(id);

            if (loaner == null)
                return NotFound();

            return Ok(loaner);
        }

        /*[HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(
            int id,
            UpdateLoanerDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != id.ToString())
                return Forbid();

            var result = await _loanerService.UpdateAsync(id, dto);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != id.ToString())
                return Forbid();

            var deleted = await _loanerService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var loanerId))
                return Unauthorized();

            try
            {
                await _loanerService.ChangePasswordAsync(loanerId, dto);

                return Ok(new
                {
                    message = "Password changed successfully."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }*/
    }
}