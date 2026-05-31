using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static LibraryAPI.Services.LoanerService;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILoanerService _loanerService;

        public AuthController(ILoanerService loanerService)
        {
            _loanerService = loanerService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterLoanerDto dto)
        {
            try
            {
                var created = await _loanerService.RegisterAsync(dto);
                return Ok(created);
            }
            catch (DuplicateEmailException ex)
            {
                return Conflict(new
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
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var result = await _loanerService.LoginAsync(dto);

                if (!result.Success)
                    return Unauthorized(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                return Ok(new { message = "Client should delete the stored token." });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
