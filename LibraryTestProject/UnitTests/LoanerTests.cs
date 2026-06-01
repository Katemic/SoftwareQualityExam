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
public class LoanerTests
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
    //first name
    [TestMethod]
    public async Task RegisterAsync_EmptyFirstName_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.FirstName = "";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_NullFirstName_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.FirstName = null;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_FirstNameTooShort_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.FirstName = "B";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_NumberInFirstName_ThrowsException()
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
    public async Task RegisterAsync_FirstNameTooLong_ThrowsException(string name)
    {
        StartMock();
        var dto = ValidDto();
        dto.FirstName = name;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("Алексей")]
    [DataRow("李明")]
    [DataRow("محمد")]
    public async Task RegisterAsync_FirstNameContainsInvalidCharacters_ThrowsException(string name)
    {
        StartMock();
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
        StartMock();
        SetupValidRegistration();

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
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.FirstName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.FirstName);
    }

    [DataTestMethod]
    [DataRow("José")]
    [DataRow("Anne Marie")]
    [DataRow("Anne-Marie")]
    [DataRow("O'Connor")]
    public async Task RegisterAsync_FirstNameSpecialCharacters_Valid(string name)
    {
        StartMock();
        SetupValidRegistration();

        var dto = ValidDto();
        dto.FirstName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.FirstName);
    }
    //last name
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
    //CPR
    [TestMethod]
    public async Task RegisterAsync_CprIsNull_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = null;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_CprIsEmpty_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_CprContainsLetters_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "InvalidCPR";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_CprInvalidDate_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "4013991234";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_CprInvalidDash_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = "010120-1234";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
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
    
    [DataTestMethod]
    [DataRow("01012012")] // 8 digits
    [DataRow("010120123")] // 9 digits
    [DataRow("01012011234")] // 11 digits
    [DataRow("010120112345")] // 12 digits
    public async Task RegisterAsync_CprLength9_ThrowsException(string cpr)
    {
        StartMock();
        var dto = ValidDto();
        dto.Cpr = cpr;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    //10 digits
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
    //email
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
    public async Task RegisterAsync_DomainTooLong_ThrowsException(int number) //todo ask about compared to total length
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
        dto.Email = "test@タ().com";

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
    [DataRow(248)] //domain 253 charators
    [DataRow(249)] //domain 254 charators
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
    //passwword
    [DataTestMethod]  // todo ask about empty with space
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
    [DataTestMethod]
    [DataRow("Passw0r")] // 7 chars
    public async Task RegisterAsync_PasswordTooShort_ThrowsException(string password)
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = password;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
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
    [DataTestMethod]
    [DataRow(63)]
    [DataRow(64)]
    public async Task RegisterAsync_PasswordTooLong_ThrowsException(int number)
    {
        StartMock();
        var dto = ValidDto();

        string invalid65 = new string('A', 1)
                 + new string('a', number)
                 + "1";

        dto.Password = invalid65;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordContainsSpaces_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "My Password123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyLetters_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "MyPassword";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyNumbers_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "1234567890";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoUppercase_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "mypassword123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoLowercase_ThrowsException()
    {
        StartMock();
        var dto = ValidDto();
        dto.Password = "MYPASSWORD123";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
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
