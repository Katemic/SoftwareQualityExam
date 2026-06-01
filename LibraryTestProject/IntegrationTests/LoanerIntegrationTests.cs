using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using LibraryTestUtilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LibraryTestProject.IntegrationTests
{
    [DoNotParallelize]
    [TestClass]
    public class LoanerIntegrationTests
    {
        private static TestDatabaseHelper CreateDatabaseHelper()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<LoanerIntegrationTests>()
                .Build();

            var connectionString = configuration.GetConnectionString("TestConnection")
                ?? throw new InvalidOperationException("Missing test database connection string.");

            return new TestDatabaseHelper(connectionString);
        }

        [TestInitialize]
        public async Task ResetDatabaseBeforeEachTest()
        {
            var databaseHelper = CreateDatabaseHelper();

            await databaseHelper.ResetAndSeedDatabaseAsync();
        }

        // Integration test:
        // Broad happy path based on Khorikov's approach.
        //
        // Scenario:
        // A valid loaner registers.
        // The loaner is saved to the database.
        // The loaner logs in using the registered credentials.
        // Login succeeds.
        [TestMethod]
        public async Task LoanerScenario_RegisterAndLogin_WorksCorrectly()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var loanerRepository = new LoanerRepository(context);

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<LoanerIntegrationTests>()
                .Build();

            var passwordHasher = new PasswordHasher<Loaner>();

            var service = new LoanerService(
                loanerRepository,
                passwordHasher,
                configuration);

            var registerDto = new RegisterLoanerDto
            {
                FirstName = "John",
                LastName = "Doe",
                Cpr = "0101901234",
                Tlf = "+45 12345678",
                Email = $"johnTester@test.com",
                Password = "Password123"
            };

            // Act
            var createdLoaner = await service.RegisterAsync(registerDto);

            // Assert database state
            var loanerFromDatabase = await context.Loaners
                .FirstAsync(x => x.Id == createdLoaner.Id);

            Assert.AreEqual(registerDto.FirstName, loanerFromDatabase.FirstName);
            Assert.AreEqual(registerDto.LastName, loanerFromDatabase.LastName);
            Assert.AreEqual(registerDto.Email, loanerFromDatabase.Email);

            var result = passwordHasher.VerifyHashedPassword(
                loanerFromDatabase,
                loanerFromDatabase.Password!,
                registerDto.Password);

            Assert.AreNotEqual(
                PasswordVerificationResult.Failed,
                result);

            Assert.AreEqual(registerDto.FirstName, loanerFromDatabase.FirstName);
            Assert.AreEqual(registerDto.LastName, loanerFromDatabase.LastName);
            Assert.AreEqual(registerDto.Email, loanerFromDatabase.Email);

        }


        // Integration test:
        // Important database-backed rejection case.
        //
        // Scenario:
        // A loaner registers successfully.
        // Another loaner attempts to register using the same email.
        // Expected result:
        // Registration is rejected.
        [TestMethod]
        public async Task LoanerScenario_EmailAlreadyExists_RejectsRegistration() // test på eksisterende i db
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var loanerRepository = new LoanerRepository(context);

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<LoanerIntegrationTests>()
                .Build();

            var passwordHasher = new PasswordHasher<Loaner>();

            var service = new LoanerService(
                loanerRepository,
                passwordHasher,
                configuration);

            var email = $"johnTested@test.com";

            var firstLoaner = new RegisterLoanerDto
            {
                FirstName = "John",
                LastName = "Doe",
                Cpr = "0101901234",
                Tlf = "+45 12345678",
                Email = email,
                Password = "Password123"
            };

            var secondLoaner = new RegisterLoanerDto
            {
                FirstName = "Jane",
                LastName = "Doe",
                Cpr = "0202901234",
                Tlf = "+45 87654321",
                Email = email,
                Password = "Password123"
            };

            await service.RegisterAsync(firstLoaner);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<LoanerService.DuplicateEmailException>(
                () => service.RegisterAsync(secondLoaner));
        }
    }
}