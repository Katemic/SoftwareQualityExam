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
public class LoanerTestsEmail
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

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task RegisterAsync_EmailRequired_ThrowsException(string email)
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("a@.c")] //4 characters
    [DataRow("a@b.d")] //5 characters
    public async Task RegisterAsync_EmailTooShort_ThrowsException(string email)
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow(51)] //255 characters
    [DataRow(52)] //256 characters
    [TestMethod]
    public async Task RegisterAsync_EmailTooLong_ThrowsException(int number)
    {
        StartMock();
        string email =
            new string('a', number) +
            "@" +
            new string('b', 200) + ".dk";

        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod] //loacal empty/too short
    public async Task RegisterAsync_LocalEmpty_ThrowsException()
    {
        StartMock();
        string email = "@test.dk";

        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow(65)] //local 65 characters
    [DataRow(66)] //local 66 characters
    public async Task RegisterAsync_LocalTooLong_ThrowsException(int number)
    {
        StartMock();
        string email =
            new string('a', number) +
            "@test.dk";

        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod] // too short
    [DataRow("test@.d")]
    [DataRow("test@t.d")]
    public async Task RegisterAsync_DomainTooShort_ThrowsException(string email)
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod] // domain empty
    public async Task RegisterAsync_DomainEmpty_ThrowsException()
    {
        StartMock();
        string email =
        "testing@";


        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow(250)] //domain 255 charators
    [DataRow(251)] //domain 256 charators
    public async Task RegisterAsync_DomainTooLong_ThrowsException(int number)
    {
        StartMock();
        string email =
        "t@" + new string('a', number) + ".dk";


        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("testexample.com")]
    [DataRow("test@@example.com")]
    public async Task RegisterAsync_InvalidAtCount_ThrowsException(string email)
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_EmailContainsSpaces_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = "Test @example.com";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("Te..st@example.com")]
    [DataRow("Test@example..com")]
    public async Task RegisterAsync_EmailContainsConsecutiveDots_ThrowsException(string email)
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow(".Test@example.com")]
    [DataRow("Test.@example.com")]
    public async Task RegisterAsync_InvalidLocalDotPlacement_ThrowsException(string email)
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("Test@.example.com")]
    [DataRow("Test@example.com.")]
    public async Task RegisterAsync_InvalidDomainDotPlacement_ThrowsException(string email)
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_DomainMissingDot_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = "aa@bce";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_InvalidLocalCharacters_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = "漢@example.com";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_InvalidDomainCharacters_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Email = "test@タ.com";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("a@b.ce")] //4 charators
    [DataRow("a@b.com")] //5 charators
    public async Task RegisterAsync_EmailValidShortValid(string email)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = email;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(email, result.Email);
    }
    [DataTestMethod]
    [DataRow(63)] //local 63 charators
    [DataRow(64)] //local 64 charators
    public async Task RegisterAsync_LocalValidLong(int number)
    {
        StartMock();
        SetupValidRegistration();

        string email =
            new string('a', number) +
            "@test.dk";

        var dto = ValidDto();
        dto.Email = email;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(email, result.Email);
    }
    [DataTestMethod]
    [DataRow(249)] //domain 252 charators
    //[DataRow(250)] //domain 253 charators -- Not applicable because total length would be 255 which exceeds limit
    public async Task RegisterAsync_DomainValidLong(int number)
    {
        StartMock();
        SetupValidRegistration();

        string email =
        "t@" + new string('a', number) + ".dk";

        var dto = ValidDto();
        dto.Email = email;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_LocalValidDot()
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "Te.st@example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_DomainCanHaveSubdomain()
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "test@mail.example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_LocalWithNumbers()
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "Test123@example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_DomainWithNumbers()
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "Test@123example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_EmailNeedsAt()
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "Test@example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }

    [DataTestMethod] // valid special characters in email
    [DataRow("Te+st@example.com")]
    [DataRow("Te!st@example.com")]
    [DataRow("Te#st@example.com")]
    [DataRow("Te$st@example.com")]
    [DataRow("Te%st@example.com")]
    [DataRow("Te&st@example.com")]
    [DataRow("Te'st@example.com")]
    [DataRow("Te*st@example.com")]
    [DataRow("Te-st@example.com")]
    [DataRow("Te/st@example.com")]
    [DataRow("Te=st@example.com")]
    [DataRow("Te?st@example.com")]
    [DataRow("Te^st@example.com")]
    [DataRow("Te_st@example.com")]
    public async Task RegisterAsync_ValidEmailCharaters(string email)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = email;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(email, result.Email);
    }
}
