using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
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
            if (string.IsNullOrWhiteSpace(dto.FirstName))
                throw new ArgumentException("First name is required.");

            if (dto.FirstName.Length < 2 || dto.FirstName.Length > 100)
                throw new ArgumentException("First name must be between 2 and 100 characters.");

            if (!Regex.IsMatch(dto.FirstName, @"^[A-Za-zÀ-ÿ' -]+$"))
                throw new ArgumentException("First name contains invalid characters.");

            if (string.IsNullOrWhiteSpace(dto.LastName))
                throw new ArgumentException("Last name is required.");

            if (dto.LastName.Length < 2 || dto.LastName.Length > 100)
                throw new ArgumentException("Last name must be between 2 and 100 characters.");

            if (!Regex.IsMatch(dto.LastName, @"^[A-Za-zÀ-ÿ' -]+$"))
                throw new ArgumentException("Last name contains invalid characters.");

            if (string.IsNullOrWhiteSpace(dto.Cpr))
                throw new ArgumentException("CPR is required.");

            if (!Regex.IsMatch(dto.Cpr, @"^\d{10}$"))
                throw new ArgumentException("CPR must contain exactly 10 digits.");

            var datePart = dto.Cpr.Substring(0, 6);

            if (!DateTime.TryParseExact(datePart, "ddMMyy", null, System.Globalization.DateTimeStyles.None, out _))
                throw new ArgumentException("CPR contains invalid date.");

            if (string.IsNullOrWhiteSpace(dto.Tlf))
                throw new ArgumentException("Phone number is required.");

            if (dto.Tlf.Count(c => c == '+') != 1)
                throw new ArgumentException("Phone number must contain exactly one '+' sign.");

            if (!dto.Tlf.StartsWith('+'))
                throw new ArgumentException("Phone number must start with '+'.");

            var split = dto.Tlf.Split(' ');

            if (split.Length != 2)
                throw new ArgumentException("Phone number must contain exactly one whitespace.");

            string countryPart = split[0];
            string subscriber = split[1];

            string countryCode = countryPart.Substring(1);

            if (countryCode.Length < 1 || countryCode.Length > 3)
                throw new ArgumentException("Country code must be between 1 and 3 digits.");

            if (!countryCode.All(char.IsDigit))
                throw new ArgumentException("Country code must contain only digits.");

            if (!subscriber.All(char.IsDigit))
                throw new ArgumentException("Subscriber number must contain only digits.");

            int totalDigits = countryCode.Length + subscriber.Length;

            if (totalDigits < 7 || totalDigits > 15)
                throw new ArgumentException("Phone number must contain between 7 and 15 digits.");

            if (subscriber.All(c => c == '0'))
                throw new ArgumentException("Subscriber number cannot contain only zeros.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Email is required.");

            if (dto.Email.Length < 6 || dto.Email.Length > 254)
                throw new ArgumentException("Email length is invalid.");

            if (dto.Email.Count(c => c == '@') != 1)
                throw new ArgumentException("Email must contain exactly one '@'.");

            var parts = dto.Email.Split('@');

            string local = parts[0];
            string domain = parts[1];

            if (string.IsNullOrWhiteSpace(local))
                throw new ArgumentException("Missing local part.");

            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Missing domain.");

            if (dto.Email.Contains(" "))
                throw new ArgumentException("Email cannot contain spaces.");

            if (dto.Email.Contains(".."))
                throw new ArgumentException("Email cannot contain consecutive dots.");

            if (local.StartsWith(".") || local.EndsWith("."))
                throw new ArgumentException("Local part cannot start or end with dot.");

            if (domain.StartsWith(".") || domain.EndsWith("."))
                throw new ArgumentException("Domain cannot start or end with dot.");

            if (!domain.Contains("."))
                throw new ArgumentException("Domain must contain '.'.");

            if (local.Length < 1 || local.Length > 64)
                throw new ArgumentException("Local part length is invalid.");

            if (domain.Length < 4 || domain.Length > 253)
                throw new ArgumentException("Domain part length is invalid.");

            if (!Regex.IsMatch(local, @"^[A-Za-z0-9!#$%&'*+/=?^_`{|}~.-]+$"))
                throw new ArgumentException("Local part contains invalid characters.");

            if (!Regex.IsMatch(domain, @"^[A-Za-z0-9.-]+$"))
                throw new ArgumentException("Domain contains invalid characters.");

            var labels = domain.Split('.');

            foreach (var label in labels)
            {
                if (string.IsNullOrWhiteSpace(label))
                    throw new ArgumentException("Invalid domain label.");

                if (label.StartsWith("-") || label.EndsWith("-"))
                    throw new ArgumentException("Domain labels cannot start or end with '-'.");
            }

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new ArgumentException("Password is required.");

            if (dto.Password.Length < 8 || dto.Password.Length > 64)
                throw new ArgumentException("Password must be between 8 and 64 characters.");

            if (dto.Password.Contains(" "))
                throw new ArgumentException("Password cannot contain spaces.");

            if (!dto.Password.Any(char.IsUpper) || !dto.Password.Any(char.IsLower) || !dto.Password.Any(char.IsDigit))
                throw new ArgumentException("Password must contain uppercase, lowercase, and number.");

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
        public async Task<LoanerDto?> UpdateAsync(int id, UpdateLoanerDto dto)
        {
            var loaner = await _loanerRepository.GetByIdAsync(id);

            if (loaner == null)
                return null;

            loaner.FirstName = dto.FirstName;
            loaner.LastName = dto.LastName;
            loaner.Tlf = dto.Tlf;
            loaner.Email = dto.Email;
            

            await _loanerRepository.UpdateAsync(loaner);

            return MapToDto(loaner);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            return await _loanerRepository.DeleteAsync(id);
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
        public async Task ChangePasswordAsync(int loanerId, ChangePasswordDto dto)
        {
            var loaner = await _loanerRepository.GetByIdAsync(loanerId);

            var result = _passwordHasher.VerifyHashedPassword(loaner, loaner.Password!, dto.CurrentPassword);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException();

            loaner.Password = _passwordHasher.HashPassword(loaner, dto.NewPassword);

            await _loanerRepository.UpdateAsync(loaner);
        }
    }
}
