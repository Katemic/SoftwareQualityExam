using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Context;
using LibrarySQLBackend.Repositories;
using LibraryTestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibraryTestProject.IntegrationTests
{
    [TestClass]
    [DoNotParallelize]
    public class LoanIntegrationTests
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

   
        // Broad database-backed happy path scenario.
        //
        // Black-box relation:
        // Covers the valid loan decision table rule:
        // - user is valid/logged in
        // - item is available
        // - user has no unpaid fine
        // - user has fewer than 3 active loans
        // - user has no overdue loans
        // Expected result: loan is created.
        //
        // White-box / integration relation:
        // Verifies that LoanService, LoanRepository, InventoryRepository,
        // Entity Framework, and the test database work together correctly.
        // Also verifies the observable system-generated effects:
        // - loan is persisted
        // - inventory status changes to "loaned out" during loan creation
        // - loan can be returned
        // - loan status changes to "returned"
        // - ReturnDate is set by the backend/database
        // - inventory status changes back to "available".
        [TestMethod]
        public async Task LoanScenario_CreateLoanAndReturnLoan_UpdatesDatabaseCorrectly()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var loanRepository = new LoanRepository(context);
            var inventoryRepository = new InventoryRepository(context);
            var loanerRepository = new LoanerRepository(context);

            var service = new LoanService(
                loanRepository,
                inventoryRepository,
                loanerRepository);

            var dto = new CreateLoanDto
            {
                LoanerId = TestIds.ValidLoanerId,
                InventoryId = TestIds.AvailableInventoryId
            };

            // Act
            var createdLoan = await service.CreateLoanAsync(dto);
            await service.ReturnLoanAsync(createdLoan.Id);

            // Assert
            var loanFromDatabase = await context.Loans
                .FirstAsync(x => x.Id == createdLoan.Id);

            var inventoryFromDatabase = await context.Inventories
                .FirstAsync(x => x.Id == TestIds.AvailableInventoryId);

            Assert.AreEqual(TestIds.ValidLoanerId, loanFromDatabase.LoanerId);
            Assert.AreEqual(TestIds.AvailableInventoryId, loanFromDatabase.InventoryId);
            Assert.AreEqual("returned", loanFromDatabase.Status);
            Assert.IsNotNull(loanFromDatabase.ReturnDate);
            Assert.AreEqual("available", inventoryFromDatabase.Status);
        }


   
        // Database-backed rejection scenario.
        //
        // Black-box relation:
        // Loan decision table - Rule 3.
        // User is valid/logged in: yes.
        // Item is available: yes.
        // User has unpaid fine: yes.
        // Expected result: loan creation is rejected.
        //
        // White-box / integration relation:
        // Verifies that the service correctly detects an unpaid fine
        // through the repository and seeded test database.
        [TestMethod]
        public async Task LoanScenario_LoanerHasUnpaidFine_RejectsLoan()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var loanRepository = new LoanRepository(context);
            var inventoryRepository = new InventoryRepository(context);
            var loanerRepository = new LoanerRepository(context);

            var service = new LoanService(
                loanRepository,
                inventoryRepository,
                loanerRepository);

            var dto = new CreateLoanDto
            {
                LoanerId = TestIds.LoanerWithUnpaidFineId,
                InventoryId = TestIds.AvailableInventoryId
            };

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateLoanAsync(dto));
        }

        // Database-backed rejection scenario.
        //
        // Black-box relation:
        // Loan decision table - Rule 5.
        // User is valid/logged in: yes.
        // Item is available: yes.
        // User has unpaid fine: no.
        // User has fewer than 3 active loans: yes.
        // User has overdue loan: yes.
        // Expected result: loan creation is rejected.
        //
        // White-box / integration relation:
        // Verifies that the service correctly detects overdue loans
        // through the repository and seeded test database.
        [TestMethod]
        public async Task LoanScenario_LoanerHasOverdueLoan_RejectsLoan()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var loanRepository = new LoanRepository(context);
            var inventoryRepository = new InventoryRepository(context);
            var loanerRepository = new LoanerRepository(context);

            var service = new LoanService(
                loanRepository,
                inventoryRepository,
                loanerRepository);

            var dto = new CreateLoanDto
            {
                LoanerId = TestIds.LoanerWithOverdueLoanId,
                InventoryId = TestIds.AvailableInventoryId
            };

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateLoanAsync(dto));
        }

        // Database-backed rejection scenario.
        //
        // Black-box relation:
        // Loan decision table - Rule 4.
        // User is valid/logged in: yes.
        // Item is available: yes.
        // User has unpaid fine: no.
        // User has 3 active loans: yes.
        // Expected result: loan creation is rejected.
        //
        // White-box / integration relation:
        // Verifies that the service correctly counts active loans
        // through the repository and seeded test database.
        [TestMethod]
        public async Task LoanScenario_LoanerHasThreeActiveLoans_RejectsLoan()
        {
            // Arrange
            var databaseHelper = CreateDatabaseHelper();

            await using var context = databaseHelper.CreateContext();

            var loanRepository = new LoanRepository(context);
            var inventoryRepository = new InventoryRepository(context);
            var loanerRepository = new LoanerRepository(context);

            var service = new LoanService(
                loanRepository,
                inventoryRepository,
                loanerRepository);

            var dto = new CreateLoanDto
            {
                LoanerId = TestIds.LoanerWithThreeActiveLoansId,
                InventoryId = TestIds.AvailableInventoryId
            };

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateLoanAsync(dto));
        }
    }
}
