using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibraryTestProject
{
    [TestClass]
    [DoNotParallelize]
    public class LoanIntegrationTests
    {
        [TestInitialize]
        public async Task ResetDatabaseBeforeEachTest()
        {
            await TestDatabaseHelper.ResetAndSeedDatabaseAsync();
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
            await using var context = TestDatabaseHelper.CreateContext();

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
            await using var context = TestDatabaseHelper.CreateContext();

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
            await using var context = TestDatabaseHelper.CreateContext();

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
    }
}
