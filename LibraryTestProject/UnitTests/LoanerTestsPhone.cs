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
public class LoanerTestsPhone
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

    //tlf
    [DataTestMethod]
    [DataRow("")]
    [DataRow(null)]
    public async Task RegisterAsync_PhoneRequired_ThrowsException(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("+4512345678")]
    [DataRow("+45  12345678")]
    public async Task RegisterAsync_InvalidPhoneWhitespace_Throws(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+45 12345")]                // 5 digits
    [DataRow("+45 123456")]               // 6 digits
    [DataRow("+45 1234567890123456")]     // 16 digits
    [DataRow("+45 12345678901234567")]    // 17 digits
    public async Task RegisterAsync_InvalidTotalDigitLength_ThrowsException(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+ 12345678")]
    public async Task RegisterAsync_CountryCodeTooShort_Throws(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+34A 12345678")]
    public async Task RegisterAsync_CountryCodeHasChar_Throws(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+34 12V45678")]
    public async Task RegisterAsync_NumberHasChar_Throws(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+1234 12345678")]
    public async Task RegisterAsync_CountryCodeTooLong_Throws(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_CountryCodeNoPlus_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = "45 12345678";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PlusNotInStart_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = "45+ 12345678";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("++45 12345678")]
    [DataRow("+++45 12345678")]
    public async Task RegisterAsync_CountryCodeTooManyPlus_ThrowsException(string phone)
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_AllZeroSubscriber_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Tlf = "+45 00000000";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+45 1234567")]             // 7 digits
    [DataRow("+45 12345678")]           // 8 digits
    [DataRow("+45 12345678901234")]      // 14 digits
    [DataRow("+45 123456789012345")]     // 15 digits
    public async Task RegisterAsync_TotalDigitCount_Valid(string phone)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Tlf = phone;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(phone, result.Tlf);
    }
    [DataTestMethod]
    [DataRow("+1 4123456789")]
    [DataRow("+354 12345678")]
    public async Task RegisterAsync_ValidPhone_ReturnsLoaner(string phone)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Tlf = phone;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(phone, result.Tlf);
    }
}
