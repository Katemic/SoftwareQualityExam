using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using LibraryTestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibraryTestProject.IntegrationTests
{
    [TestClass]
    [DoNotParallelize]
    public class FineIntegrationTests
    {
        private static TestDatabaseHelper CreateDatabaseHelper()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<LoanIntegrationTests>()
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

        // Integration test - happy path / decision table Rule 4:
        // Tests that FineService, FineRepository, LoanRepository,
        // AppDbContext and MySQL work together.
        //
        // Scenario:
        // The loan exists, the loan is overdue,
        // and the loan does not already have a fine.
        // Expected result:
        // A new fine is saved in the database with status "unpaid".
        [TestMethod]
        public async Task CreateAsync_LoanWithoutFine_SavesFineInDatabase()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();
            var service = CreateService(context);

            var dto = new CreateFineDto
            {
                LoanId = TestIds.LoanWithoutFineId,
                Amount = 20
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            var savedFine = await context.Fines
                .FirstAsync(f => f.Id == result.Id);

            Assert.AreEqual("unpaid", savedFine.Status);
            Assert.AreEqual(TestIds.LoanWithoutFineId, savedFine.LoanId);
        }

        // Integration test - happy path:
        // Tests that FineService, FineRepository,
        // AppDbContext and MySQL work together.
        //
        // Scenario:
        // An unpaid fine exists in the database.
        // Expected result:
        // The fine can be paid, and its status is updated to "paid" in the database.
        [TestMethod]
        public async Task PayFineAsync_UnpaidFine_UpdatesFineStatusInDatabase()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();
            var service = CreateService(context);

            // Act
            await service.PayFineAsync(TestIds.UnpaidFineId);

            // Assert
            var fine = await context.Fines
                .FirstAsync(f => f.Id == TestIds.UnpaidFineId);

            Assert.AreEqual("paid", fine.Status);
        }


        // Creates the real service with real repositories.
        // No mocks are used here because this is an integration test.
        private static FineService CreateService(AppDbContext context)
        {
            var fineRepository = new FineRepository(context);
            var loanRepository = new LoanRepository(context);

            return new FineService(fineRepository, loanRepository);
        }
    }
}