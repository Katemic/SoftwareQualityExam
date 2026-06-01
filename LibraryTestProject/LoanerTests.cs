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

namespace LibraryTestProject;

[TestClass]
public class LoanerTests
{
    private Mock<ILoanerRepository> _repoMock = null!;
    private Mock<IConfiguration> _configMock = null!;
    private Mock<IPasswordHasher<Loaner>> _passwordHasherMock = null!;
    private LoanerService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _repoMock = new Mock<ILoanerRepository>();
        _configMock = new Mock<IConfiguration>();
        _passwordHasherMock = new Mock<IPasswordHasher<Loaner>>();

        _service = new LoanerService(
            _repoMock.Object,
            _passwordHasherMock.Object,
            _configMock.Object);
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

    [DataTestMethod]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoK")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKi")]
    public async Task RegisterAsync_FirstNameTooLong_ThrowsException(string name)
    {
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
        SetupValidRegistration();

        var dto = ValidDto();
        dto.FirstName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.FirstName);
    }

    [DataTestMethod]
    [DataRow("José")]
    [DataRow("Anne-Marie")]
    [DataRow("Anne-Marie")]
    [DataRow("O'Connor")]
    public async Task RegisterAsync_FirstNameSpecialCharacters_Valid(string name)
    {
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
        var dto = ValidDto();
        dto.LastName = "";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_NullLastName_ThrowsException()
    {
        var dto = ValidDto();
        dto.LastName = null;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [TestMethod]
    public async Task RegisterAsync_LastNameTooShort_ThrowsException()
    {
        var dto = ValidDto();
        dto.LastName = "B";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }

    [DataTestMethod]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoK")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKi")]
    public async Task RegisterAsync_LastNameTooLong_ThrowsException(string name)
    {
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
        SetupValidRegistration();

        var dto = ValidDto();
        dto.LastName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.LastName);
    }

    [DataTestMethod]
    [DataRow("José")]
    [DataRow("Anne-Marie")]
    [DataRow("Anne-Marie")]
    [DataRow("O'Connor")]
    public async Task RegisterAsync_LastNameSpecialCharacters_Valid(string name)
    {
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
        dto.Cpr = "InvalidCPR";

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
        var dto = ValidDto();
        dto.Cpr = cpr;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    //10 digits
    [TestMethod]
    public async Task RegisterAsync_CprLength10_IsValid()
    {
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Cpr = "0101201234";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Cpr, result.Cpr);
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
    public async Task RegisterAsync_CountryCodeTooShort_Throws(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+34A 12345678")]
    public async Task RegisterAsync_CountryCodeHasChar_Throws(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+3 12V45678")]
    public async Task RegisterAsync_NumberHasChar_Throws(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("+1234 12345678")]
    [DataRow("+12345 12345678")]
    public async Task RegisterAsync_CountryCodeTooLong_Throws(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod]
    public async Task RegisterAsync_CountryCodeNoPlus_ThrowsException()
    {
        var dto = ValidDto();
        dto.Tlf = "45 12345678";

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataRow("++45 12345678")]
    [DataRow("+++45 12345678")]
    [TestMethod]
    public async Task RegisterAsync_CountryCodeTooManyPlus_ThrowsException(string phone)
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
    [DataRow("+1 1234567")]           // 8 digits
    [DataRow("+45 123456789012")]      // 14 digits
    [DataRow("+45 1234567890123")]     // 15 digits
    public async Task RegisterAsync_TotalDigitCount_Valid(string phone)
    {
        SetupValidRegistration();

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
    [DataRow(" ")]
    public async Task RegisterAsync_EmailRequired_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("a@c.")] //4 charators
    [DataRow("a@b.d")] //5 charators
    public async Task RegisterAsync_EmailTooShort_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow(51)] //255 charators
    [DataRow(52)] //256 charators
    [TestMethod]
    public async Task RegisterAsync_EmailTooLong_ThrowsException(int number)
    {
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
        string email = "@test.dk";

        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow(65)] //local 65 charators
    [DataRow(66)] //local 66 charators
    public async Task RegisterAsync_LocalTooLong_ThrowsException(int number)
    {
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
        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [TestMethod] // domain empty
    public async Task RegisterAsync_DomainEmpty_ThrowsException()
    {
        string email =
        "test@.dk";


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
        string email =
        "test@" + new string('a', number) + ".dk";


        var dto = ValidDto();
        dto.Email = email;

        await Assert.ThrowsExceptionAsync<ArgumentException>(
    () => _service.RegisterAsync(dto));
    }
    [DataTestMethod]
    [DataRow("testexample.com")]
    [DataRow("test@@example.com")]
    [DataRow("test@@@example.com")]
    public async Task RegisterAsync_InvalidAtCount_ThrowsException(string email)
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
    [DataRow("Te...st@example.com")]
    [DataRow("Test@example..com")]
    [DataRow("Test@example...com")]
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
    [DataRow("a@c.dk")] //4 charators
    [DataRow("a@b.com")] //5 charators
    public async Task RegisterAsync_EmailValidShortValid(string email)
    {
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
    [DataRow("test@t.dk")] // 4 charators
    [DataRow("test@te.dk")] // 5 charators
    public async Task RegisterAsync_DomainValidShort(string email)
    {
        SetupValidRegistration();

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
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "Te.st@example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_DomainCanHaveSubdomain()
    {
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "test@mail.example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_LocalWithNumbers()
    {
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "Test123@example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_DomainWithNumbers()
    {
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Email = "Test@123example.com";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Email, result.Email);
    }
    [TestMethod]
    public async Task RegisterAsync_EmailNeedsAt()
    {
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
        SetupValidRegistration();

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
    [DataTestMethod]
    [DataRow("Passw0r")] // 6 chars
    [DataRow("Passw0")] // 7 chars
    public async Task RegisterAsync_PasswordTooShort_ThrowsException(string password)
    {
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
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Password = password;

        var result = await _service.RegisterAsync(dto);

        Assert.IsNotNull(result);
    }
    [DataTestMethod]
    [DataRow(63)]
    [DataRow(64)]
    public async Task RegisterAsync_PasswordLength65_ThrowsException(int number)
    {
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
        SetupValidRegistration();

        var dto = ValidDto();
        dto.Password = password;

        var result = await _service.RegisterAsync(dto);

        Assert.IsNotNull(result);
    }
}
