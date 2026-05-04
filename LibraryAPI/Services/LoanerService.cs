using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryAPI.Services
{
    public class LoanerService : ILoanerService
    {
        private readonly ILoanerRepository _loanerRepository;
        private readonly IPasswordHasher<Loaner> _passwordHasher;
        private readonly IConfiguration _configuration;

        public LoanerService(
            ILoanerRepository loanerRepository,
            IPasswordHasher<Loaner> passwordHasher,
            IConfiguration configuration)
        {
            _loanerRepository = loanerRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<IEnumerable<LoanerDto>> GetAllAsync()
        {
            var loaners = await _loanerRepository.GetAllAsync();

            return loaners.Select(MapToDto);
        }

        public async Task<LoanerDto?> GetByIdAsync(int id)
        {
            var loaner = await _loanerRepository.GetByIdAsync(id);
            return loaner == null ? null : MapToDto(loaner);
        }

        public async Task<LoanerDto> RegisterAsync(RegisterLoanerDto dto)
        {
            var existing = await _loanerRepository.GetByEmailAsync(dto.Email!);
            if (existing != null)
                throw new ArgumentException("A user with this email already exists.");

            var loaner = new Loaner
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Cpr = dto.Cpr,
                Tlf = dto.Tlf,
                Email = dto.Email
            };

            loaner.Password = _passwordHasher.HashPassword(loaner, dto.Password!);

            var created = await _loanerRepository.AddAsync(loaner);
            return MapToDto(created);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var loaner = await _loanerRepository.GetByEmailAsync(dto.Email!);

            if (loaner == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var result = _passwordHasher.VerifyHashedPassword(
                loaner,
                loaner.Password!,
                dto.Password!);

            if (result == PasswordVerificationResult.Failed)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            var token = GenerateJwtToken(loaner, out var expiresAtUtc);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                User = MapToDto(loaner)
            };
        }

        private string GenerateJwtToken(Loaner loaner, out DateTime expiresAtUtc)
        {
            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Missing Jwt:Key");

            var jwtIssuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Missing Jwt:Issuer");

            var jwtAudience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("Missing Jwt:Audience");

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, loaner.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, loaner.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, loaner.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{loaner.FirstName} {loaner.LastName}".Trim())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            expiresAtUtc = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static LoanerDto MapToDto(Loaner loaner)
        {
            return new LoanerDto
            {
                Id = loaner.Id,
                FirstName = loaner.FirstName,
                LastName = loaner.LastName,
                Cpr = loaner.Cpr,
                Tlf = loaner.Tlf,
                Email = loaner.Email
            };
        }
    }
}
