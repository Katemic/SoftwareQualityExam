using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using Moq;

namespace LibraryTestProject.UnitTests;

[TestClass]
public class LoanUnitTests
{

    // Black-box test:
    // Extra validation case: loaner does not exist.
    // Expected result: loan cannot be created.
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

    // Black-box test:
    // Extra validation case: inventory copy does not exist.
    // Expected result: loan cannot be created.
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
    // Decision table - Rule 2.
    // User is logged in: yes.
    // Item is available: no.
    // Expected result: item unavailable.
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
    // Decision table - Rule 3.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: yes.
    // Expected result: loan rejected.
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
    // Decision table - Rule 4.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: no.
    // User has 3 active loans: yes.
    // Expected result: too many loans.
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
    // Decision table - Rule 5.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: no.
    // User has 3 active loans: no.
    // User has overdue loans: yes.
    // Expected result: return your item.
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
    // Decision table - Rule 6.
    // User is logged in: yes.
    // Item is available: yes.
    // User has unpaid fine: no.
    // User has 3 active loans: no.
    // User has overdue loans: no.
    // Expected result: loan created.
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


    // Black-box test:
    // Return date equivalence partition.
    // Input value: NULL.
    // Expected result: valid when creating loan.
    [TestMethod]
    public async Task CreateLoanAsync_AllRulesAreValid_SetsReturnDateToNull()
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

        SetupValidLoanCreation(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock,
            dto);

        // Act
        var result = await service.CreateLoanAsync(dto);

        // Assert
        Assert.IsNull(result.ReturnDate);
    }

    // Black-box test:
    // Status equivalence partition.
    // Input value: active.
    // Expected result: valid.
    [TestMethod]
    public async Task CreateLoanAsync_AllRulesAreValid_SetsStatusToActive()
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

        SetupValidLoanCreation(
            loanRepositoryMock,
            inventoryRepositoryMock,
            loanerRepositoryMock,
            dto);

        // Act
        var result = await service.CreateLoanAsync(dto);

        // Assert
        Assert.AreEqual("active", result.Status);
    }

    // Black-box test:
    // Extra validation case: loan does not exist.
    // Expected result: loan cannot be returned.
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

    // Black-box test:
    // Return date equivalence partition.
    // Input value: loan already has return date.
    // Expected result: invalid.
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

    // Black-box test:
    // Return date equivalence partition.
    // Input value before return: NULL.
    // Expected result: valid, because a loan with no return date can be returned.
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

    // Black-box test:
    // Extra validation case.
    // Input value: LoanerId <= 0.
    // Expected result: invalid input.
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

    // Black-box test:
    // Extra validation case.
    // Input value: InventoryId <= 0.
    // Expected result: invalid input.
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
