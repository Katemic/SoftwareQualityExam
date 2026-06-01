using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LibraryTestProject.UnitTests
{
    [TestClass]
    public class FineServiceTests
    {
        // Black-box boundary value test:
        // Invalid input: fine amount is on or below the lower boundary.
        // Expected result: fine creation is rejected.
        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        public async Task CreateAsync_InvalidFineAmount_ThrowsException(int amount)
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = amount
            };

            var loan = CreateLoan(dto.LoanId, "active");

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box boundary value test / decision table Rule 4:
        // Valid input: fine amount is above the lower boundary.
        // Valid fine scenario: the loan exists, the loan is overdue,
        // and the loan does not already have a fine.
        // Expected result: fine is created with the specified amount.
        [DataTestMethod]
        [DataRow(1)]
        [DataRow(2)]
        public async Task CreateAsync_ValidFineAmount_CreatesFine(int amount)
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = amount
            };

            var loan = CreateLoan(dto.LoanId, "overdue");

            SetupValidFineCreation(
                fineRepositoryMock,
                loanRepositoryMock,
                dto,
                loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual(amount, result.Amount);
        }

        // Black-box format test:
        // Valid input: fine amount is a decimal number.
        // Business rules are satisfied: the loan exists, is overdue,
        // and does not already have a fine.
        // Expected result: fine is created with the specified decimal amount.
        [TestMethod]
        public async Task CreateAsync_WhenAmountIsDecimal_CreatesFine()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20.5m
            };

            var loan = CreateLoan(dto.LoanId, "overdue");

            SetupValidFineCreation(
                fineRepositoryMock,
                loanRepositoryMock,
                dto,
                loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual(20.5m, result.Amount);
        }


        // Black-box equivalence partition test:
        // Valid outcome: a newly created fine receives the default unpaid status.
        // Expected result: fine status is set to "unpaid".
        [TestMethod]
        public async Task CreateAsync_WhenFineCreated_StatusIsUnpaid()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20
            };

            var loan = CreateLoan(dto.LoanId, "overdue");

            SetupValidFineCreation(
                fineRepositoryMock,
                loanRepositoryMock,
                dto,
                loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual("unpaid", result.Status);
        }

        // Black-box business rule test:
        // Valid scenario: an existing unpaid fine can be paid.
        // Expected result: fine status is changed to "paid".
        [TestMethod]
        public async Task PayFineAsync_WhenFineIsUnpaid_SetsStatusToPaid()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var fine = CreateFine(id: 1, loanId: 1, status: "unpaid");

            fineRepositoryMock
                .Setup(x => x.GetByIdAsync(fine.Id))
                .ReturnsAsync(fine);

            fineRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Fine>()))
                .ReturnsAsync(true);

            // Act
            await service.PayFineAsync(fine.Id);

            // Assert
            Assert.AreEqual("paid", fine.Status);
        }

        // Black-box business rule test:
        // Invalid scenario: the fine has already been paid.
        // Expected result: payment is rejected because a fine can only be paid once.
        [TestMethod]
        public async Task PayFineAsync_WhenFineAlreadyPaid_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var fine = CreateFine(id: 1, loanId: 1, status: "paid");
            fine.PaidDate = DateTime.Now;

            fineRepositoryMock
                .Setup(x => x.GetByIdAsync(fine.Id))
                .ReturnsAsync(fine);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.PayFineAsync(fine.Id));
        }


        // Black-box decision table test:
        // Rule 1: the loan does not exist.
        // Expected result: fine creation is rejected because a fine can only
        // be given for an existing loan.
        [TestMethod]
        public async Task CreateAsync_WhenLoanNotFound_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 999,
                Amount = 20
            };

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync((Loan?)null);

            // Act + Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));

            Assert.AreEqual("Loan not found.", exception.Message);
        }

        // Black-box decision table test:
        // Rule 2: the loan exists, but the loan is not overdue.
        // Expected result: the loaner cannot receive a fine.
        [TestMethod]
        public async Task CreateAsync_WhenLoanIsNotOverdue_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20
            };

            var loan = CreateLoan(dto.LoanId, "active");

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            // Act + Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));

            Assert.AreEqual("Loan is not overdue.", exception.Message);
        }

        // Black-box decision table test:
        // Rule 3: the loan exists and is overdue,
        // but the loan already has an unpaid fine.
        // Expected result: another fine cannot be created for the same loan.
        [TestMethod]
        public async Task CreateAsync_WhenLoanAlreadyHasUnpaidFine_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20
            };

            var loan = CreateLoan(dto.LoanId, "overdue");

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            fineRepositoryMock
                .Setup(x => x.GetByLoanIdAsync(dto.LoanId))
                .ReturnsAsync(new List<Fine>
                {
            CreateFine(id: 1, loanId: dto.LoanId, status: "unpaid")
                });

            // Act + Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));

            Assert.AreEqual("Loan already has a fine.", exception.Message);
        }

        // Black-box decision table test:
        // Rule 3: the loan exists and is overdue,
        // but the loan already has a paid fine.
        // Expected result: another fine cannot be created for the same loan,
        // because a loan can only have one fine regardless of payment status.
        [TestMethod]
        public async Task CreateAsync_WhenLoanAlreadyHasPaidFine_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20
            }; 

            var loan = CreateLoan(dto.LoanId, "overdue");

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            fineRepositoryMock
                .Setup(x => x.GetByLoanIdAsync(dto.LoanId))
                .ReturnsAsync(new List<Fine>
                {
            CreateFine(id: 1, loanId: dto.LoanId, status: "paid")
                });

            // Act + Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));

            Assert.AreEqual("Loan already has a fine.", exception.Message);
        }


        // White-box statement coverage test:
        // Invalid branch: the fine lookup returns null.
        // Expected result: payment is rejected with "Fine not found."
        [TestMethod]
        public async Task PayFineAsync_WhenFineNotFound_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            int fineId = 999;

            fineRepositoryMock
                .Setup(x => x.GetByIdAsync(fineId))
                .ReturnsAsync((Fine?)null);

            // Act + Assert
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.PayFineAsync(fineId));

            Assert.AreEqual("Fine not found.", exception.Message);
        }

        private static FineService CreateService(
            Mock<IFineRepository> fineRepositoryMock,
            Mock<ILoanRepository> loanRepositoryMock)
        {
            return new FineService(
                fineRepositoryMock.Object,
                loanRepositoryMock.Object);
        }

        private static Loan CreateLoan(int id, string status)
        {
            return new Loan
            {
                Id = id,
                Status = status
            };
        }

        private static Fine CreateFine(int id, int loanId, string status)
        {
            return new Fine
            {
                Id = id,
                LoanId = loanId,
                Amount = 20,
                Status = status,
            };
        }

        private static void SetupValidFineCreation(
            Mock<IFineRepository> fineRepositoryMock,
            Mock<ILoanRepository> loanRepositoryMock,
            CreateFineDto dto,
            Loan loan)
        {
            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            fineRepositoryMock
                .Setup(x => x.GetByLoanIdAsync(dto.LoanId))
                .ReturnsAsync(new List<Fine>());

            fineRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Fine>()))
                .ReturnsAsync((Fine fine) =>
                {
                    fine.Id = 1;
                    return fine;
                });
        }
    }
}