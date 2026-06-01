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
        // Black-box boundary test:
        // Invalid partition: fine amount less than or equal to 0.
        // Expected result: fine cannot be created.
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

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            loanRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.LoanId))
                .ReturnsAsync(loan);

            // Act + Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => service.CreateAsync(dto));
        }

        // Black-box boundary test:
        // Valid partition: amount greater than 0.
        // Expected result: fine is created with specified amount.
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

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

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
        // Invalid format partition: text input.
        // Expected result: amount cannot be parsed as integer.
        [TestMethod]
        public void FineAmount_TextInput_IsInvalid()
        {
            // Arrange
            string input = "abc";

            // Act
            var canParse = int.TryParse(input, out _);

            // Assert
            Assert.IsFalse(canParse);
        }

        // Black-box format test:
        // Invalid format partition: special characters.
        // Expected result: amount cannot be parsed as integer.
        [TestMethod]
        public void FineAmount_SpecialCharacters_IsInvalid()
        {
            // Arrange
            string input = "!#€";

            // Act
            var canParse = int.TryParse(input, out _);

            // Assert
            Assert.IsFalse(canParse);
        }

        // Black-box equivalence partition test:
        // Valid partition: fine is unpaid when created.
        // Expected result: status is set to unpaid.
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

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

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

        // Black-box equivalence partition test:
        // Invalid partition: fine is already paid when created.
        // Expected result: status cannot be paid on creation.
        [TestMethod]
        public async Task CreateAsync_WhenFineCreated_StatusCannotBePaid()
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

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(
                fineRepositoryMock,
                loanRepositoryMock,
                dto,
                loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreNotEqual("paid", result.Status);
        }

        // Black-box test:
        // Valid test case: created date is today.
        // Expected result: created date is valid and set to today.
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
                Amount = 20
            };

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(
                fineRepositoryMock,
                loanRepositoryMock,
                dto,
                loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.AreEqual(DateTime.Now.Date, result.CreatedDate.Date);
        }

        // Black-box test:
        // Valid test case: paid date is set when unpaid fine is paid.
        // Expected result: paid date is not null and is today.
        [TestMethod]
        public async Task PayFineAsync_WhenFineIsUnpaid_SetsPaidDateToToday()
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
            Assert.IsNotNull(fine.PaidDate);
            Assert.AreEqual(DateTime.Now.Date, fine.PaidDate.Value.Date);
        }

        // Black-box test:
        // Valid test case: paid date is null before fine is paid.
        // Expected result: newly created fine has no paid date.
        [TestMethod]
        public async Task CreateAsync_WhenFineCreated_PaidDateIsNull()
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

            var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

            SetupValidFineCreation(
                fineRepositoryMock,
                loanRepositoryMock,
                dto,
                loan);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.IsNull(result.PaidDate);
        }

        // Black-box business rule test:
        // Invalid partition: fine is already paid.
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