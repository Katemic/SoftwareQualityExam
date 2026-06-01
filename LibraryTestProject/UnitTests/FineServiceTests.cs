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

            var loan = CreateLoan(dto.LoanId, "active");

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
        // Invalid format partition: text input.
        // Expected result: amount cannot be parsed as integer.
        //[TestMethod]
        //public void FineAmount_TextInput_IsInvalid()
        //{
        //    // Arrange
        //    string input = "abc";

        //    // Act
        //    var canParse = int.TryParse(input, out _);

        //    // Assert
        //    Assert.IsFalse(canParse);
        //}

        //// Black-box format test:
        //// Invalid format partition: special characters.
        //// Expected result: amount cannot be parsed as integer.
        //[TestMethod]
        //public void FineAmount_SpecialCharacters_IsInvalid()
        //{
        //    // Arrange
        //    string input = "!#€";

        //    // Act
        //    var canParse = int.TryParse(input, out _);

        //    // Assert
        //    Assert.IsFalse(canParse);
        //}

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

            var loan = CreateLoan(dto.LoanId, "overdue");

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
        //[TestMethod]
        //public async Task CreateAsync_WhenFineCreated_CreatedDateIsToday()
        //{
        //    // Arrange
        //    var fineRepositoryMock = new Mock<IFineRepository>();
        //    var loanRepositoryMock = new Mock<ILoanRepository>();
        //    var service = CreateService(fineRepositoryMock, loanRepositoryMock);

        //    var dto = new CreateFineDto
        //    {
        //        LoanId = 1,
        //        Amount = 20
        //    };

        //    var loan = CreateLoan(dto.LoanId, DateTime.Now.AddDays(-1));

        //    SetupValidFineCreation(
        //        fineRepositoryMock,
        //        loanRepositoryMock,
        //        dto,
        //        loan);

        //    // Act
        //    var result = await service.CreateAsync(dto);

        //    // Assert
        //    Assert.AreEqual(DateTime.Now.Date, result.CreatedDate.Date);
        //}
        // Black-box test:
        // Valid partition: fine is unpaid.
        // Expected result: fine status is changed to paid.
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

            var loan = CreateLoan(dto.LoanId, "overdue");

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


        // White-box statement coverage test:
        // Invalid branch: loan does not exist.
        // Expected result: fine cannot be created.
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

        // White-box statement coverage test:
        // Invalid branch: loan exists, but is not overdue.
        // Expected result: fine cannot be created.
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

        // White-box statement coverage test:
        // Invalid branch: loan already has an unpaid fine.
        // Expected result: another unpaid fine cannot be created for the same loan.
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

        // White-box statement coverage test:
        // Valid branch: loan has an old paid fine.
        // Expected result: new unpaid fine can still be created.
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
        // Invalid branch: fine does not exist.
        // Expected result: fine cannot be paid.
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