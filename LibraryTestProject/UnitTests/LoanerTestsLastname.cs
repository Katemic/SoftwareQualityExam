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
public class LoanerTestsLastname
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

    [TestMethod]
    public async Task RegisterAsync_EmptyLastName_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.LastName = "";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_NullLastName_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.LastName = null;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_LastNameTooShort_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.LastName = "B";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_NumberInLastName_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.FirstName = "Kristoffer1";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoK")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKi")]
    public async Task RegisterAsync_LastNameTooLong_ThrowsException(string name)
    {
        StartMock();
        var dto = ValidDto();
        dto.LastName = name;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("Алексей")]
    [DataRow("李明")]
    [DataRow("محمد")]
    public async Task RegisterAsync_LastNameContainsInvalidCharacters_ThrowsException(string name)
    {
        StartMock();
        var dto = ValidDto();
        dto.LastName = name;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("Bo")]
    [DataRow("Bob")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikk")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikko")]
    public async Task RegisterAsync_LastNameLength_Valid(string name)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.LastName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.LastName);
    }

    [DataTestMethod]
    [DataRow("kristoffer")]
    [DataRow("KRISTOFFER")]
    [DataRow("KrisTOfFeR")]
    public async Task RegisterAsync_LastNameCapitalization_Valid(string name)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.LastName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.LastName);
    }

    [DataTestMethod]
    [DataRow("José")]
    [DataRow("Anne Marie")]
    [DataRow("Anne-Marie")]
    [DataRow("O'Connor")]
    public async Task RegisterAsync_LastNameSpecialCharacters_Valid(string name)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.LastName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.LastName);
    }
}
