using LibraryAPI.DTOs;
using LibraryAPI.Services;
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

        // Integration test:
        // Broad happy path based on Khorikov's approach.
        //
        // Scenario:
        // A valid loaner borrows an available inventory copy.
        // The loan is saved to the database.
        // The inventory copy becomes loaned out.
        // The same loan is returned.
        // The loan status becomes returned.
        // The return date is saved.
        // The inventory copy becomes available again.
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

        // Integration test:
        // Date rules for the loan happy path.
        //
        // Scenario:
        // A valid loaner creates a loan.
        // The created loan gets LoanDate set to the current day.
        // The created loan gets DueDate set to LoanDate + 14 days.
        // The created loan has ReturnDate = null.
        // The loan is returned.
        // The returned loan gets ReturnDate set.
        // The ReturnDate is greater than or equal to the LoanDate.
        [TestMethod]
        public async Task LoanScenario_CreateLoanAndReturnLoan_SetsDatesCorrectly()
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

            var loanAfterCreate = await context.Loans
                .AsNoTracking()
                .FirstAsync(x => x.Id == createdLoan.Id);

            await service.ReturnLoanAsync(createdLoan.Id);

            var loanAfterReturn = await context.Loans
                .AsNoTracking()
                .FirstAsync(x => x.Id == createdLoan.Id);

            // Assert
            Assert.AreEqual(DateTime.Now.Date, loanAfterCreate.LoanDate.Date);
            Assert.AreEqual(loanAfterCreate.LoanDate.AddDays(14).Date, loanAfterCreate.DueDate.Date);
            Assert.IsNull(loanAfterCreate.ReturnDate);
            Assert.IsNotNull(loanAfterReturn.ReturnDate);
            Assert.IsTrue(loanAfterReturn.ReturnDate >= loanAfterReturn.LoanDate);
        }

        // Integration test:
        // Important database-backed rejection case.
        //
        // Scenario:
        // A loaner with an unpaid fine tries to create a loan.
        // Expected result:
        // The loan is rejected.
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

        // Integration test:
        // Important database-backed rejection case.
        //
        // Scenario:
        // A loaner with an overdue loan tries to create a loan.
        // Expected result:
        // The loan is rejected.
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

        // Integration test:
        // Important database-backed rejection case.
        //
        // Scenario:
        // A loaner already has 3 active loans in the database.
        // The loaner tries to create another loan.
        // Expected result:
        // The loan is rejected.
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
