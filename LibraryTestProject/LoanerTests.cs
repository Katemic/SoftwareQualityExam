using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace LibraryTestProject;

[TestClass]
public class LoanerTests
{
    private IConfiguration _configuration = null!;
    private AppDbContext _context = null!;
    private LoanerService _service = null!;
    private IDbContextTransaction _transaction;

    [TestInitialize]
    public async Task Setup()
    {
        _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.Test.json")
                .Build();

        var connectionString =
            _configuration.GetConnectionString("TestDatabase");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString))
            .Options;

        _context = new AppDbContext(options);

        var repository = new LoanerRepository(_context);
        var passwordHasher = new PasswordHasher<Loaner>();

        _service = new LoanerService(
            repository,
            passwordHasher,
            _configuration);
        _transaction = await _context.Database.BeginTransactionAsync();
    }
    [TestCleanup]
    public async Task Cleanup()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }

        if (_context != null)
        {
            await _context.DisposeAsync();
        }
    }

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

    [TestMethod]
    public async Task RegisterAsync_EmptyFirstName_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName = "";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_NullFirstName_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName = null;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_FirstNameTooShort_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName = "B";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_FirstNameTooLong_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName =
            "KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikko" +
            "KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoK";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("Алексей")]
    [DataRow("李明")]
    [DataRow("محمد")]
    public async Task RegisterAsync_FirstNameContainsInvalidCharacters_ThrowsException(string name)
    {
        var dto = ValidDto();
        dto.FirstName = name;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("Bo")]
    [DataRow("Bob")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikk")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikko")]
    public async Task RegisterAsync_FirstNameLength_Valid(string name)
    {
        var dto = ValidDto();
        dto.FirstName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.FirstName);
    }

    [DataTestMethod]
    [DataRow("kristoffer")]
    [DataRow("KRISTOFFER")]
    [DataRow("KrisTOfFeR")]
    public async Task RegisterAsync_FirstNameCapitalization_Valid(string name)
    {
        var dto = ValidDto();
        dto.FirstName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.FirstName);
    }

    [DataTestMethod]
    [DataRow("José")]
    [DataRow("Anne-Marie O'Connor")] //split i 3
    public async Task RegisterAsync_FirstNameSpecialCharacters_Valid(string name)
    {
        var dto = ValidDto();
        dto.FirstName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.FirstName);
    }
    //CPR
    [TestMethod]
    public async Task RegisterAsync_CprIsNull_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = null;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_CprIsEmpty_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_CprContainsLetters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "MyCPRNumber";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_CprInvalidDate_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "4013991234";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_CprValid_ReturnsLoaner()
    {
        var dto = ValidDto();
        dto.Cpr = "0101201234";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Cpr, result.Cpr);
    }
    [TestMethod] // kombiner med 11
    public async Task RegisterAsync_CprLength9_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "010120123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_CprLength10_IsValid()
    {
        var dto = ValidDto();
        dto.Cpr = "0101201234";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Cpr, result.Cpr);
    }
    [TestMethod]
    public async Task RegisterAsync_CprLength11_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "01012011234";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    //tlf
    [DataTestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task RegisterAsync_PhoneRequired_ThrowsException(string phone)
    {
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
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+45 123")]                // 5 digits
    [DataRow("+45 1234")]               // 6 digits
    [DataRow("+45 12345678901234")]     // 16 digits
    [DataRow("+45 123456789012345")]    // 17 digits
    public async Task RegisterAsync_InvalidTotalDigitLength_ThrowsException(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+ 12345678")]
    [DataRow("+1234 12345678")]
    public async Task RegisterAsync_InvalidCountryCode_Throws(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_AllZeroSubscriber_ThrowsException()
    {
        var dto = ValidDto();
        dto.Tlf = "+45 00000000";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+1 123456")]             // 7 digits
    [DataRow("+45 1234567")]           // 9 digits
    [DataRow("+45 123456789012")]      // 14 digits
    [DataRow("+45 1234567890123")]     // 15 digits
    public async Task RegisterAsync_TotalDigitCount_Valid(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(phone, result.Tlf);
    }
    [DataTestMethod]
    [DataRow("+1 4123456789")]
    [DataRow("+45 12345678")]
    [DataRow("+354 12345678")]
    public async Task RegisterAsync_ValidPhone_ReturnsLoaner(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(phone, result.Tlf);
    }
    //email
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public async Task RegisterAsync_EmailRequired_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("a@c")]
    [DataRow("a@b")]
    public async Task RegisterAsync_EmailTooShort_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod] // split i 2 og behold total length 
    public async Task RegisterAsync_EmailTooLong_ThrowsException()
    {
        string email =
            new string('a', 64) +
            "@" +
            new string('b', 250);

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
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("@example.com")]
    public async Task RegisterAsync_MissingLocalPart_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("testin8@")]
    public async Task RegisterAsync_MissingDomainPart_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_EmailContainsSpaces_ThrowsException()
    {
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
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_DomainMissingDot_ThrowsException()
    {
        var dto = ValidDto();
        dto.Email = "test@com";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_InvalidLocalCharacters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Email = "漢@example.com";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_InvalidDomainCharacters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Email = "test@タ().com";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("a@b.ce")]
    [DataRow("Test@example.com")]
    [DataRow("Te.st@example.com")]
    [DataRow("Test@exam.ple.com")]
    [DataRow("Test123@example.com")]
    [DataRow("Test@123example.com")]
    [DataRow("Te+!#$%&'*+-/=?^_st@example.com")] // split
    [DataRow("test.user@example.com")]
    [DataRow("test@mail.example.com")]
    public async Task RegisterAsync_ValidEmail_ReturnsLoaner(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(email, result.Email);
    }
    //passwword
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public async Task RegisterAsync_PasswordRequired_ThrowsException(string password)
    {
        var dto = ValidDto();
        dto.Password = password;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod] // tilføj 9
    public async Task RegisterAsync_PasswordLength7_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "Passw0r"; // 7 chars

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("Passw0rd")] // 8
    [DataRow("Password1")]
    public async Task RegisterAsync_ValidPasswordLengths(string password)
    {
        var dto = ValidDto();
        dto.Password = password;

        var result = await _service.RegisterAsync(dto);

        Assert.IsNotNull(result);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordLength65_ThrowsException()
    {
        var dto = ValidDto();

        string invalid65 = new string('A', 1)
                 + new string('a', 63)
                 + "1";

        dto.Password = invalid65;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordContainsSpaces_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "My Password123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyLetters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "Thisismypassword";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyNumbers_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "1234567890";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoUppercase_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "mypassword123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoLowercase_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "MYPASSWORD123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoNumber_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "MyPassword";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("Passw0rd")]
    [DataRow("Password1")]
    [DataRow("Secure123Password")]
    public async Task RegisterAsync_ValidPassword_ReturnsLoaner(string password)
    {
        var dto = ValidDto();
        dto.Password = password;

        var result = await _service.RegisterAsync(dto);

        Assert.IsNotNull(result);
    }
}
