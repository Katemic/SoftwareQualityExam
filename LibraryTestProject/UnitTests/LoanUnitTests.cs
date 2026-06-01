using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Moq;

namespace LibraryTestProject.UnitTests;

[TestClass]
public class LoanUnitTests
{

    // White-box / service-layer validation test:
    // Scenario: the supplied LoanerId does not match an existing loaner.
    // This is an additional defensive validation case, not part of the main
    // black-box loan decision table.
    // Expected result: loan creation is rejected.
    [TestMethod]
    public async Task CreateLoanAsync_LoanerDoesNotExist_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto();

        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync((Loaner?)null);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => service.CreateLoanAsync(dto));
    }

    // White-box / service-layer validation test:
    // Scenario: the supplied InventoryId does not match an existing inventory copy.
    // This supports the business rule that a loan can only be created for
    // an existing item copy.
    // Expected result: loan creation is rejected.
    [TestMethod]
    public async Task CreateLoanAsync_InventoryDoesNotExist_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto();

        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync(CreateLoaner());

        inventoryRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.InventoryId))
            .ReturnsAsync((Inventory?)null);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            () => service.CreateLoanAsync(dto));
    }

    // Black-box test:
    // Loan decision table - Rule 2.
    // User is logged in: yes.
    // Item is available: no.
    // Expected result: loan creation is rejected because the item is unavailable.
    [TestMethod]
    public async Task CreateLoanAsync_ItemUnavailable_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto();

        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync(CreateLoaner());

        inventoryRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.InventoryId))
            .ReturnsAsync(CreateInventory(status: "loaned out"));

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.CreateLoanAsync(dto));
    }

    // Black-box test:
    // Loan decision table - Rule 3.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: yes.
    // Expected result: loan creation is rejected.
    [TestMethod]
    public async Task CreateLoanAsync_LoanerHasUnpaidFine_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto();

        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync(CreateLoaner());

        inventoryRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.InventoryId))
            .ReturnsAsync(CreateInventory(status: "available"));

        loanRepositoryMock
            .Setup(x => x.HasUnpaidFineAsync(dto.LoanerId))
            .ReturnsAsync(true);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.CreateLoanAsync(dto));
    }

    // Black-box test:
    // Loan decision table - Rule 4.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: no.
    // User has 3 active loans: yes.
    // Expected result: loan creation is rejected because the user has too many active loans.
    [TestMethod]
    public async Task CreateLoanAsync_LoanerHasThreeActiveLoans_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto();

        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync(CreateLoaner());

        inventoryRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.InventoryId))
            .ReturnsAsync(CreateInventory(status: "available"));

        loanRepositoryMock
            .Setup(x => x.HasUnpaidFineAsync(dto.LoanerId))
            .ReturnsAsync(false);

        loanRepositoryMock
            .Setup(x => x.CountActiveLoansAsync(dto.LoanerId))
            .ReturnsAsync(3);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.CreateLoanAsync(dto));
    }

    // Black-box test:
    // Loan decision table - Rule 5.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: no.
    // User has fewer than 3 active loans: yes.
    // User has overdue loans: yes.
    // Expected result: loan creation is rejected because the user must return overdue items first.
    [TestMethod]
    public async Task CreateLoanAsync_LoanerHasOverdueLoan_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto();

        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync(CreateLoaner());

        inventoryRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.InventoryId))
            .ReturnsAsync(CreateInventory(status: "available"));

        loanRepositoryMock
            .Setup(x => x.HasUnpaidFineAsync(dto.LoanerId))
            .ReturnsAsync(false);

        loanRepositoryMock
            .Setup(x => x.CountActiveLoansAsync(dto.LoanerId))
            .ReturnsAsync(2);

        loanRepositoryMock
            .Setup(x => x.HasOverdueLoanAsync(
                dto.LoanerId))
            .ReturnsAsync(true);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.CreateLoanAsync(dto));
    }

    // Black-box test:
    // Loan decision table - Rule 6.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: no.
    // User has fewer than 3 active loans: yes.
    // User has overdue loans: no.
    // Expected result: loan is created successfully.
    [TestMethod]
    public async Task CreateLoanAsync_AllRulesAreValid_CreatesLoan()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto();

        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync(CreateLoaner());

        inventoryRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.InventoryId))
            .ReturnsAsync(CreateInventory(status: "available"));

        loanRepositoryMock
            .Setup(x => x.HasUnpaidFineAsync(dto.LoanerId))
            .ReturnsAsync(false);

        loanRepositoryMock
            .Setup(x => x.CountActiveLoansAsync(dto.LoanerId))
            .ReturnsAsync(2);

        loanRepositoryMock
            .Setup(x => x.HasOverdueLoanAsync(
                dto.LoanerId))
            .ReturnsAsync(false);

        loanRepositoryMock
            .Setup(x => x.CreateLoanAsync(It.IsAny<Loan>()))
            .ReturnsAsync((Loan loan) =>
            {
                loan.Id = 1;
                return loan;
            });

        // Act
        var result = await service.CreateLoanAsync(dto);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(dto.LoanerId, result.LoanerId);
        Assert.AreEqual(dto.InventoryId, result.InventoryId);
        Assert.AreEqual("active", result.Status);
        Assert.IsNull(result.ReturnDate);

        loanRepositoryMock.Verify(
            x => x.CreateLoanAsync(It.IsAny<Loan>()),
            Times.Once);
    }

    // White-box / service-layer validation test:
    // Scenario: the supplied LoanId does not match an existing loan.
    // Expected result: the loan cannot be returned.
    [TestMethod]
    public async Task ReturnLoanAsync_LoanDoesNotExist_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        loanRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Loan?)null);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.ReturnLoanAsync(1));
    }

    // White-box:
    // Scenario: the loan already has status "returned" and a ReturnDate.
    // A returned loan should not be returned again.
    // Expected result: return operation is rejected.
    [TestMethod]
    public async Task ReturnLoanAsync_LoanIsAlreadyReturned_ThrowsException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        loanRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreateLoan(
                returnDate: DateTime.Now,
                status: "returned"));

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.ReturnLoanAsync(1));
    }

    // White-box:
    // Scenario: the loan is active and has no ReturnDate.
    // This represents a loan that has not been returned yet.
    // Expected result: the service calls the repository return method exactly once.
    [TestMethod]
    public async Task ReturnLoanAsync_LoanIsNotReturned_CallsRepositoryReturnLoan()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        loanRepositoryMock
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(CreateLoan(
                returnDate: null,
                status: "active"));

        // Act
        await service.ReturnLoanAsync(1);

        // Assert
        loanRepositoryMock.Verify(
            x => x.ReturnLoanAsync(1),
            Times.Once);
    }

    // White-box / input validation test:
    // Scenario: LoanerId is 0, which is outside the valid ID range.
    // This is an additional service-layer validation case.
    // Expected result: ArgumentException for LoanerId.
    [TestMethod]
    public async Task CreateLoanAsync_LoanerIdIsZero_ThrowsArgumentException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto(
            loanerId: 0,
            inventoryId: 1);

        // Act + Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => service.CreateLoanAsync(dto));

        Assert.AreEqual("LoanerId", exception.ParamName);
    }

    // White-box / input validation test:
    // Scenario: InventoryId is 0, which is outside the valid ID range.
    // This is an additional service-layer validation case.
    // Expected result: ArgumentException for InventoryId.
    [TestMethod]
    public async Task CreateLoanAsync_InventoryIdIsZero_ThrowsArgumentException()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var inventoryRepositoryMock = new Mock<IInventoryRepository>();
        var loanerRepositoryMock = new Mock<ILoanerRepository>();

        var service = CreateService(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock);

        var dto = CreateLoanDto(
            loanerId: 1,
            inventoryId: 0);

        // Act + Assert
        var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => service.CreateLoanAsync(dto));

        Assert.AreEqual("InventoryId", exception.ParamName);
    }

    private static LoanService CreateService(
        Mock<ILoanRepository> loanRepositoryMock,
        Mock<IInventoryRepository> inventoryRepositoryMock,
        Mock<ILoanerRepository> loanerRepositoryMock)
    {
        return new LoanService(
            loanRepositoryMock.Object,
            inventoryRepositoryMock.Object,
            loanerRepositoryMock.Object);
    }

    private static CreateLoanDto CreateLoanDto(
        int loanerId = 1,
        int inventoryId = 1)
    {
        return new CreateLoanDto
        {
            LoanerId = loanerId,
            InventoryId = inventoryId
        };
    }

    private static Loaner CreateLoaner(int id = 1)
    {
        return new Loaner
        {
            Id = id,
            Email = "test@test.dk",
            Password = "password"
        };
    }

    private static Inventory CreateInventory(
        int id = 1,
        string status = "available")
    {
        return new Inventory
        {
            Id = id,
            ItemId = 1,
            Barcode = "ABC123",
            Status = status
        };
    }

    private static Loan CreateLoan(
        int id = 1,
        int loanerId = 1,
        int inventoryId = 1,
        DateTime? returnDate = null,
        string status = "active")
    {
        return new Loan
        {
            Id = id,
            LoanerId = loanerId,
            InventoryId = inventoryId,
            ReturnDate = returnDate,
            Status = status
        };
    }

    private static void SetupValidLoanCreation(
        Mock<ILoanRepository> loanRepositoryMock,
        Mock<IInventoryRepository> inventoryRepositoryMock,
        Mock<ILoanerRepository> loanerRepositoryMock,
        CreateLoanDto dto)
    {
        loanerRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.LoanerId))
            .ReturnsAsync(CreateLoaner());

        inventoryRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.InventoryId))
            .ReturnsAsync(CreateInventory(status: "available"));

        loanRepositoryMock
            .Setup(x => x.HasUnpaidFineAsync(dto.LoanerId))
            .ReturnsAsync(false);

        loanRepositoryMock
            .Setup(x => x.CountActiveLoansAsync(dto.LoanerId))
            .ReturnsAsync(0);

        loanRepositoryMock
            .Setup(x => x.HasOverdueLoanAsync(
                dto.LoanerId))
            .ReturnsAsync(false);

        loanRepositoryMock
            .Setup(x => x.CreateLoanAsync(It.IsAny<Loan>()))
            .ReturnsAsync((Loan loan) =>
            {
                loan.Id = 1;
                return loan;
            });
    }

}
