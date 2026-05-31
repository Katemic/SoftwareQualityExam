using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LibraryTestProject.Services
{
    [TestClass]
    public class FineServiceTests
    {
        // Black-box test:
        // Valid amount partition: amount greater than 0.
        // Expected result: backend sets amount to 20.
        [TestMethod]
        public async Task CreateAsync_WhenFineCreated_AmountIs20()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(fineRepositoryMock, loanRepositoryMock, dto, loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual(20m, result.Amount);
        }

        // Black-box parameterized test:
        // Invalid amount partitions.
        // 0 = amount equal to 0.
        // -20 = negative amount.
        // Expected result: fine cannot be created.
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(-20)]
        public async Task CreateAsync_InvalidAmount_ThrowsException(int amount)
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = amount,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            fineRepositoryMock
                .Setup(x => x.GetByLoanIdAsync(dto.LoanId))
                .ReturnsAsync(new List<Fine>());

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box boundary test:
        // Boundary value: amount equals -1.
        // Expected result: fine cannot be created.
        [TestMethod]
        public async Task CreateAsync_AmountMinusOne_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = -1,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            fineRepositoryMock
                .Setup(x => x.GetByLoanIdAsync(dto.LoanId))
                .ReturnsAsync(new List<Fine>());

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box boundary test:
        // Boundary value: amount equals 1.
        // Expected result: fine is created.
        [TestMethod]
        public async Task CreateAsync_AmountOne_CreatesFine()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 1,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(fineRepositoryMock, loanRepositoryMock, dto, loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual(20m, result.Amount);
        }

        // Black-box boundary test:
        // Boundary value: amount equals 2.
        // Expected result: fine is created.
        [TestMethod]
        public async Task CreateAsync_AmountTwo_CreatesFine()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 2,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(fineRepositoryMock, loanRepositoryMock, dto, loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual(20m, result.Amount);
        }

        // Black-box test:
        // Valid payment status partition.
        // Expected result: new fine has status unpaid.
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
                Amount = 20,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(fineRepositoryMock, loanRepositoryMock, dto, loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual("unpaid", result.Status);
        }

        // Black-box test:
        // Valid payment partition: fine is unpaid.
        // Expected result: status changes to paid and paid date is set.
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
            Assert.IsNotNull(fine.PaidDate);
        }

        // Black-box test:
        // Invalid status partition: fine status is neither unpaid nor paid.
        // Expected result: only unpaid fines can be paid.
        [TestMethod]
        public async Task PayFineAsync_WhenFineStatusIsInvalid_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var fine = CreateFine(id: 1, loanId: 1, status: "late");

            fineRepositoryMock
                .Setup(x => x.GetByIdAsync(fine.Id))
                .ReturnsAsync(fine);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.PayFineAsync(fine.Id));
        }

        // Black-box test:
        // Created date valid partition.
        // Expected result: created date is today.
        [TestMethod]
        public async Task CreateAsync_WhenFineCreated_CreatedDateIsToday()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(fineRepositoryMock, loanRepositoryMock, dto, loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual(DateTime.Now.Date, result.CreatedDate.Date);
        }

        // Black-box test:
        // Invalid created date partition.
        // Expected result: created date cannot be in the future.
        [TestMethod]
        public async Task CreateAsync_CreatedDateInFuture_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20,
                CreatedDate = DateTime.Now.AddDays(1)
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            fineRepositoryMock
                .Setup(x => x.GetByLoanIdAsync(dto.LoanId))
                .ReturnsAsync(new List<Fine>());

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box test:
        // Invalid created date partition.
        // Expected result: created date cannot be before due date.
        [TestMethod]
        public async Task CreateAsync_CreatedDateBeforeDueDate_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20,
                CreatedDate = DateTime.Now.AddDays(-2)
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box parameterized boundary test:
        // Invalid boundary values for due date.
        // 0 days = due today, not overdue.
        // +1 day = due in future, not overdue.
        [DataTestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public async Task CreateAsync_DueDateNotPassed_ThrowsException(int daysFromToday)
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(daysFromToday));

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box parameterized boundary test:
        // Valid boundary values for overdue loans.
        // -1 day and -2 days are overdue and should create fine.
        [DataTestMethod]
        [DataRow(-1)]
        [DataRow(-2)]
        public async Task CreateAsync_DueDatePassed_CreatesFine(int daysFromToday)
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(daysFromToday));

            SetupValidFineCreation(fineRepositoryMock, loanRepositoryMock, dto, loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual("unpaid", result.Status);
            Assert.AreEqual(20m, result.Amount);
        }

        // Black-box test:
        // Invalid partition: loan does not exist.
        // Expected result: fine cannot be created.
        [TestMethod]
        public async Task CreateAsync_LoanDoesNotExist_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 999,
                Amount = 20,
                CreatedDate = DateTime.Now
            };

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync((Loan?)null);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box test:
        // Invalid partition: loan already has unpaid fine.
        // Expected result: duplicate unpaid fine is rejected.
        [TestMethod]
        public async Task CreateAsync_WhenUnpaidFineAlreadyExists_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            var dto = new CreateFineDto
            {
                LoanId = 1,
                Amount = 20,
                CreatedDate = DateTime.Now
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            var existingFines = new List<Fine>
            {
                CreateFine(id: 1, loanId: dto.LoanId, status: "unpaid")
            };

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            fineRepositoryMock
                .Setup(x => x.GetByLoanIdAsync(dto.LoanId))
                .ReturnsAsync(existingFines);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box test:
        // Invalid payment partition: fine does not exist.
        // Expected result: payment is rejected.
        [TestMethod]
        public async Task PayFineAsync_FineDoesNotExist_ThrowsException()
        {
            // Arrange
            var fineRepositoryMock = new Mock<IFineRepository>();
            var loanRepositoryMock = new Mock<ILoanRepository>();
            var service = CreateService(fineRepositoryMock, loanRepositoryMock);

            fineRepositoryMock
                .Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Fine?)null);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.PayFineAsync(999));
        }

        // Black-box test:
        // Invalid payment partition: fine already paid.
        // Expected result: fine cannot be paid twice.
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

        private static FineService CreateService(
            Mock<IFineRepository> fineRepositoryMock,
            Mock<ILoanRepository> loanRepositoryMock)
        {
            return new FineService(
                fineRepositoryMock.Object,
                loanRepositoryMock.Object);
        }

        private static Loan CreateLoan(int id, DateTime dueDate)
        {
            return new Loan
            {
                Id = id,
                DueDate = dueDate
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
                CreatedDate = DateTime.Now
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