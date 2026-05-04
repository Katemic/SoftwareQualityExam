using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            var created = await _loanerService.RegisterAsync(dto);
            return Ok(created);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _loanerService.LoginAsync(dto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { message = "Client should delete the stored token." });
        }
    }
}
