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
public class LoanerTestsCPR
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
    // CPR cannot be null
    [TestMethod]
    public async Task RegisterAsync_CprIsNull_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = null;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // CPR cannot be empty
    [TestMethod]
    public async Task RegisterAsync_CprIsEmpty_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // CPR may only contain numeric digits
    [TestMethod]
    public async Task RegisterAsync_CprContainsLetters_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "InvalidCPR";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // CPR must contain a date in the first six digits
    [TestMethod]
    public async Task RegisterAsync_CprInvalidDate_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "4013991234";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // CPR must be entered without separators such as dashes
    [TestMethod]
    public async Task RegisterAsync_CprInvalidDash_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "010120-1234";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // Valid 10-digit CPR should register successfully
    [TestMethod]
    public async Task RegisterAsync_CprValid_ReturnsLoaner()
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Cpr = "0101201234";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Cpr, result.Cpr);
    }
    // CPR must contain exactly 10 digits
    [DataTestMethod]
    [DataRow("01012012")] // 8 digits
    [DataRow("010120123")] // 9 digits
    [DataRow("01012011234")] // 11 digits
    [DataRow("010120112345")] // 12 digits
    public async Task RegisterAsync_CprLength_ThrowsException(string cpr)
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = cpr;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    // CPR with exactly 10 digits should be accepted
    [TestMethod]
    public async Task RegisterAsync_CprLength10_IsValid()
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Cpr = "0101201234";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Cpr, result.Cpr);
    }

}
