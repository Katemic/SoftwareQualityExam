using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LibraryTestProject;

[TestClass]
public class LoanerTests
{
    private Mock<ILoanerRepository> _loanerRepository = null!;
    private Mock<IPasswordHasher<Loaner>> _passwordHasher = null!;
    private Mock<IConfiguration> _configuration = null!;
    private LoanerService _service = null!;


    [TestInitialize]
    public void Setup()
    {
        _loanerRepository = new Mock<ILoanerRepository>();
        _passwordHasher = new Mock<IPasswordHasher<Loaner>>();
        _configuration = new Mock<IConfiguration>();

        _service = new LoanerService(_loanerRepository.Object, _passwordHasher.Object,_configuration.Object);
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
    private void SetupSuccessfulRegistration()
    {
        _loanerRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((Loaner?)null);

        _passwordHasher
            .Setup(h => h.HashPassword(It.IsAny<Loaner>(), It.IsAny<string>()))
            .Returns("hashed-password");

        _loanerRepository
            .Setup(r => r.AddAsync(It.IsAny<Loaner>()))
            .ReturnsAsync((Loaner loaner) => loaner);
    }

    [TestMethod]
    public async Task RegisterAsync_EmptyFirstName_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName = "";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("First name is required.", ex.Message);
    }

    [TestMethod]
    public async Task RegisterAsync_NullFirstName_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName = null;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("First name is required.", ex.Message);
    }

    [TestMethod]
    public async Task RegisterAsync_FirstNameTooShort_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName = "B";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "First name must be between 2 and 100 characters.",
            ex.Message);
    }

    [TestMethod]
    public async Task RegisterAsync_FirstNameTooLong_ThrowsException()
    {
        var dto = ValidDto();
        dto.FirstName =
            "KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikko" +
            "KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoK";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "First name must be between 2 and 100 characters.",
            ex.Message);
    }

    [DataTestMethod]
    [DataRow("Алексей")]
    [DataRow("李明")]
    [DataRow("محمد")]
    public async Task RegisterAsync_FirstNameContainsInvalidCharacters_ThrowsException(string name)
    {
        var dto = ValidDto();
        dto.FirstName = name;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "First name contains invalid characters.",
            ex.Message);
    }

    [DataTestMethod]
    [DataRow("Bo")]
    [DataRow("Bob")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikk")]
    [DataRow("KikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikkoKikko")]
    public async Task RegisterAsync_FirstNameLength_Valid(string name)
    {
        SetupSuccessfulRegistration();

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
        SetupSuccessfulRegistration();

        var dto = ValidDto();
        dto.FirstName = name;

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(name, result.FirstName);
    }

    [DataTestMethod]
    [DataRow("José")]
    [DataRow("Anne-Marie O'Connor")]
    public async Task RegisterAsync_FirstNameSpecialCharacters_Valid(string name)
    {
        SetupSuccessfulRegistration();

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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("CPR is required.", ex.Message);
    }

    [TestMethod]
    public async Task RegisterAsync_CprIsEmpty_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("CPR is required.", ex.Message);
    }

    [TestMethod]
    public async Task RegisterAsync_CprContainsLetters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "MyCPRNumber";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "CPR must contain exactly 10 digits.",
            ex.Message);
    }

    [TestMethod]
    public async Task RegisterAsync_CprInvalidDate_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "4013991234";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "CPR contains invalid date.",
            ex.Message);
    }

    [TestMethod]
    public async Task RegisterAsync_CprValid_ReturnsLoaner()
    {
        SetupSuccessfulRegistration();

        var dto = ValidDto();
        dto.Cpr = "0101201234";

        var result = await _service.RegisterAsync(dto);

        Assert.AreEqual(dto.Cpr, result.Cpr);
    }
    [TestMethod]
    public async Task RegisterAsync_CprLength9_ThrowsException()
    {
        var dto = ValidDto();
        dto.Cpr = "010120123";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "CPR must contain exactly 10 digits.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_CprLength10_IsValid()
    {
        SetupSuccessfulRegistration();

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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "CPR must contain exactly 10 digits.",
            ex.Message);
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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("Phone number is required.", ex.Message);
    }

    [DataTestMethod]
    [DataRow("+4512345678")]
    [DataRow("+45  12345678")]
    public async Task RegisterAsync_InvalidPhoneWhitespace_Throws(string phone)
    {
        var dto = ValidDto();
        dto.Tlf = phone;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Phone number must contain exactly one whitespace.",
            ex.Message);
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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Phone number must contain between 7 and 15 digits.",
            ex.Message);
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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Subscriber number cannot contain only zeros.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow("+1 123456")]             // 7 digits
    [DataRow("+45 1234567")]           // 9 digits
    [DataRow("+45 123456789012")]      // 14 digits
    [DataRow("+45 1234567890123")]     // 15 digits
    public async Task RegisterAsync_TotalDigitCount_Valid(string phone)
    {
        SetupSuccessfulRegistration();

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
        SetupSuccessfulRegistration();

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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("Email is required.", ex.Message);
    }
    [DataTestMethod]
    [DataRow("a@c")]
    [DataRow("a@b")]
    public async Task RegisterAsync_EmailTooShort_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("Email length is invalid.", ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_EmailTooLong_ThrowsException()
    {
        string email =
            new string('a', 64) +
            "@" +
            new string('b', 250);

        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("Email length is invalid.", ex.Message);
    }
    [DataTestMethod]
    [DataRow("testexample.com")]
    [DataRow("test@@example.com")]
    public async Task RegisterAsync_InvalidAtCount_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Email must contain exactly one '@'.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow("@example.com")]
    public async Task RegisterAsync_MissingLocalPart_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("Missing local part.", ex.Message);
    }
    [DataTestMethod]
    [DataRow("testin8@")]
    public async Task RegisterAsync_MissingDomainPart_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("Missing domain.", ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_EmailContainsSpaces_ThrowsException()
    {
        var dto = ValidDto();
        dto.Email = "Test @example.com";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Email cannot contain spaces.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow("Te..st@example.com")]
    [DataRow("Test@example..com")]
    public async Task RegisterAsync_EmailContainsConsecutiveDots_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Email cannot contain consecutive dots.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow(".Test@example.com")]
    [DataRow("Test.@example.com")]
    public async Task RegisterAsync_InvalidLocalDotPlacement_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Local part cannot start or end with dot.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow("Test@.example.com")]
    [DataRow("Test@example.com.")]
    public async Task RegisterAsync_InvalidDomainDotPlacement_ThrowsException(string email)
    {
        var dto = ValidDto();
        dto.Email = email;

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Domain cannot start or end with dot.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_DomainMissingDot_ThrowsException()
    {
        var dto = ValidDto();
        dto.Email = "test@com";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Domain must contain '.'.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_InvalidLocalCharacters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Email = "漢@example.com";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Local part contains invalid characters.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_InvalidDomainCharacters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Email = "test@タ().com";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Domain contains invalid characters.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow("a@b.ce")]
    [DataRow("Test@example.com")]
    [DataRow("Te.st@example.com")]
    [DataRow("Test@exam.ple.com")]
    [DataRow("Test123@example.com")]
    [DataRow("Test@123example.com")]
    [DataRow("Te+!#$%&'*+-/=?^_st@example.com")]
    [DataRow("test.user@example.com")]
    [DataRow("test@mail.example.com")]
    public async Task RegisterAsync_ValidEmail_ReturnsLoaner(string email)
    {
        SetupSuccessfulRegistration();

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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual("Password is required.", ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordLength7_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "Passw0r"; // 7 chars

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password must be between 8 and 64 characters.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow("Passw0rd")] // 8
    [DataRow("Password1")]
    public async Task RegisterAsync_ValidPasswordLengths(string password)
    {
        SetupSuccessfulRegistration();

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

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password must be between 8 and 64 characters.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordContainsSpaces_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "My Password123";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password cannot contain spaces.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyLetters_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "Thisismypassword";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password must contain uppercase, lowercase, and number.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordOnlyNumbers_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "1234567890";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password must contain uppercase, lowercase, and number.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoUppercase_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "mypassword123";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password must contain uppercase, lowercase, and number.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoLowercase_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "MYPASSWORD123";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password must contain uppercase, lowercase, and number.",
            ex.Message);
    }
    [TestMethod]
    public async Task RegisterAsync_PasswordNoNumber_ThrowsException()
    {
        var dto = ValidDto();
        dto.Password = "MyPassword";

        var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.RegisterAsync(dto));

        Assert.AreEqual(
            "Password must contain uppercase, lowercase, and number.",
            ex.Message);
    }
    [DataTestMethod]
    [DataRow("Passw0rd")]
    [DataRow("Password1")]
    [DataRow("Secure123Password")]
    public async Task RegisterAsync_ValidPassword_ReturnsLoaner(string password)
    {
        SetupSuccessfulRegistration();

        var dto = ValidDto();
        dto.Password = password;

        var result = await _service.RegisterAsync(dto);

        Assert.IsNotNull(result);
    }
}
