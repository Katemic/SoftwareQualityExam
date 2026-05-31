using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Context;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibraryTestProject.Integration
{
    [TestClass]
    public class FineIntegrationTests
    {
        private const string ConnectionString =
            "server=localhost;port=3307;database=mydb_test;user=root;password=1234";

        // Integration test - happy path:
        // Tests that FineService, FineRepository, LoanRepository,
        // AppDbContext and MySQL work together.
        // Scenario: an overdue loan without a fine gets a new unpaid fine saved in the database.
        [TestMethod]
        public async Task CreateAsync_OverdueLoanWithoutFine_SavesFineInDatabase()
        {
            // Arrange
            await using var context = CreateContext();
            var service = CreateService(context);

            var existingLoan = await context.Loans.FirstAsync();

            var testLoan = new Loan
            {
                LoanDate = DateTime.Now.AddDays(-20),
                DueDate = DateTime.Now.AddDays(-5),
                ReturnDate = null,
                Status = "overdue",
                LoanerId = existingLoan.LoanerId,
                InventoryId = existingLoan.InventoryId
            };

            context.Loans.Add(testLoan);
            await context.SaveChangesAsync();

            var dto = new CreateFineDto
            {
                LoanId = testLoan.Id
            };

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            var savedFine = await context.Fines
                .FirstAsync(f => f.Id == result.Id);

            Assert.AreEqual("unpaid", savedFine.Status);
        }

        // Integration test - happy path:
        // Tests that FineService, FineRepository, LoanRepository,
        // AppDbContext and MySQL work together.
        // Scenario: an unpaid fine is paid and the status is updated in the database.
        [TestMethod]
        public async Task PayFineAsync_UnpaidFine_UpdatesFineStatusInDatabase()
        {
            // Arrange
            await using var context = CreateContext();
            var service = CreateService(context);

            var existingLoan = await context.Loans.FirstAsync();

            var testLoan = new Loan
            {
                LoanDate = DateTime.Now.AddDays(-20),
                DueDate = DateTime.Now.AddDays(-5),
                ReturnDate = null,
                Status = "overdue",
                LoanerId = existingLoan.LoanerId,
                InventoryId = existingLoan.InventoryId
            };

            context.Loans.Add(testLoan);
            await context.SaveChangesAsync();

            var fine = new Fine
            {
                Amount = 20,
                Status = "unpaid",
                CreatedDate = DateTime.Now,
                PaidDate = null,
                LoanId = testLoan.Id
            };

            context.Fines.Add(fine);
            await context.SaveChangesAsync();

            // Act
            await service.PayFineAsync(fine.Id);

            // Assert
            await context.Entry(fine).ReloadAsync();

            Assert.AreEqual("paid", fine.Status);
        }

        // Creates a real EF Core DbContext connected to the MySQL test database.
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseMySql(
                    ConnectionString,
                    ServerVersion.AutoDetect(ConnectionString))
                .Options;

            return new AppDbContext(options);
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