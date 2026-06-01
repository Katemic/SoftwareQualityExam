using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LibraryTestProject.UnitTests;

[TestClass]
public class LoanerTestsPassword
{
    private Mock<ILoanerRepository> _repoMock = null!;
    private Mock<IConfiguration> _configMock = null!;
    private Mock<IPasswordHasher<Loaner>> _passwordHasherMock = null!;
    private LoanerService _service = null!;

    private RegisterLoanerDto ValidDto()
    {
        return new RegisterLoanerDto
        {
            FirstName = "John",
            LastName = "Doe",
            Cpr = "0101901234",
            Tlf = "+45 12345678",
            Email = "john@example.com",
            Password = "Password1"
        };
    }
    private void StartMock()
    {
        _repoMock = new Mock<ILoanerRepository>();
        _configMock = new Mock<IConfiguration>();
        _passwordHasherMock = new Mock<IPasswordHasher<Loaner>>();

        _service = new LoanerService(
            _repoMock.Object,
            _passwordHasherMock.Object,
            _configMock.Object);
    }
    private void SetupValidRegistration()
    {
        _repoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Loaner?)null);

        _passwordHasherMock
            .Setup(h => h.HashPassword(
                It.IsAny<Loaner>(),
                It.IsAny<string>()))
            .Returns("HASHED_PASSWORD");

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<Loaner>()))
            .ReturnsAsync((Loaner l) => l);
    }
    // Password cannot be null or empty
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task RegisterAsync_PasswordRequired_ThrowsException(string password)
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = password;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password can't be less than 8 characters
    [DataTestMethod]
    [DataRow("Passw0")] // 6 chars
    [DataRow("Passw0r")] // 7 chars
    public async Task RegisterAsync_PasswordTooShort_ThrowsException(string password)
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = password;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password must contain 8 characters
    [DataTestMethod]
    [DataRow("Passw0rd")] // 8
    [DataRow("Password1")]
    public async Task RegisterAsync_ValidPasswordLengths(string password)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Password = password;

        var result = await _service.RegisterAsync(dto);

        Assert.IsNotNull(result);
    }
    // Password exceeding the maximum allowed length should throw
    [DataTestMethod]
    [DataRow(63)] // 1 uppercase + 63 lowercase + 1 number = 65 chars
    [DataRow(64)] // 1 uppercase + 63 lowercase + 1 number = 66 chars
    public async Task RegisterAsync_PasswordTooLong_ThrowsException(int number)
    {
        StartMock();
        var dto = ValidDto();

        string invalidPassword = new string('A', 1)
                 + new string('a', number)
                 + "1";

        dto.Password = invalidPassword;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password may not contain whitespace characters
    [TestMethod]
    public async Task RegisterAsync_PasswordContainsSpaces_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "My Password123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password must contain at least one numeric digit
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyLetters_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "MyPassword";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password must contain both letters and numbers
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyNumbers_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "1234567890";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password must contain at least one uppercase letter
    [TestMethod]
    public async Task RegisterAsync_PasswordNoUppercase_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "mypassword123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password must contain at least one lowercase letter
    [TestMethod]
    public async Task RegisterAsync_PasswordNoLowercase_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "MYPASSWORD123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Password meeting all validation requirements should register successfully
    [DataTestMethod]
    [DataRow("Passw0rd")]
    [DataRow("Password1")]
    public async Task RegisterAsync_ValidPassword_ReturnsLoaner(string password)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Password = password;

        var result = await _service.RegisterAsync(dto);

        Assert.IsNotNull(result);
    }
}
