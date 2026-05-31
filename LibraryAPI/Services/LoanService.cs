using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using LibrarySQLBackend.Repositories.Interfaces;
using System.Data.Common;

namespace LibraryAPI.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILoanerRepository _loanerRepository;

        private const int LoanPeriodInDays = 14;
        private const int MaxActiveLoans = 3;

        public LoanService(ILoanRepository loanRepository, IInventoryRepository inventoryRepository, ILoanerRepository loanerRepository)
        {
            _loanRepository = loanRepository;
            _inventoryRepository = inventoryRepository;
            _loanerRepository = loanerRepository;
        }

        public async Task<LoanDto?> GetByIdAsync(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);

            if (loan == null)
                return null;

            return MapToDto(loan);
        }

        public async Task<LoanDto> CreateLoanAsync(CreateLoanDto dto)
        {
            if (dto.LoanerId <= 0)
                throw new ArgumentException("A valid loaner is required.", nameof(dto.LoanerId));

            if (dto.InventoryId <= 0)
                throw new ArgumentException("A valid inventory copy is required.", nameof(dto.InventoryId));

            var loaner = await _loanerRepository.GetByIdAsync(dto.LoanerId);
            if (loaner == null)
                throw new KeyNotFoundException("The selected loaner does not exist.");

            var inventory = await _inventoryRepository.GetByIdAsync(dto.InventoryId);
            if (inventory == null)
                throw new KeyNotFoundException("The selected inventory copy does not exist.");

            if (inventory.Status != "available")
                throw new InvalidOperationException("Item unavailable.");

            var hasUnpaidFine = await _loanRepository.HasUnpaidFineAsync(dto.LoanerId);
            if (hasUnpaidFine)
                throw new InvalidOperationException("Loan rejected: the loaner has an unpaid fine.");

            var activeLoans = await _loanRepository.CountActiveLoansAsync(dto.LoanerId);
            if (activeLoans >= MaxActiveLoans)
                throw new InvalidOperationException("Too many loans.");

            var hasOverdueLoan = await _loanRepository.HasOverdueLoanAsync(dto.LoanerId);
            if (hasOverdueLoan)
                throw new InvalidOperationException("Return your item.");

            var loanDate = DateTime.Now;

            var loan = new Loan
            {
                LoanerId = dto.LoanerId,
                InventoryId = dto.InventoryId,
                LoanDate = loanDate,
                DueDate = loanDate.AddDays(LoanPeriodInDays),
                ReturnDate = null,
                Status = "active"
            };

            var createdLoan = await _loanRepository.CreateLoanAsync(loan);

            return MapToDto(createdLoan);
        }

        public async Task ReturnLoanAsync(int loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);

            if (loan == null)
                throw new InvalidOperationException("The loan was not found.");

            if (loan.ReturnDate != null || loan.Status == "returned")
                throw new InvalidOperationException("The loan has already been returned.");

            await _loanRepository.ReturnLoanAsync(loanId);
        }

        private static LoanDto MapToDto(Loan loan)
        {
            return new LoanDto
            {
                Id = loan.Id,
                LoanDate = loan.LoanDate,
                DueDate = loan.DueDate,
                ReturnDate = loan.ReturnDate,
                Status = loan.Status,
                LoanerId = loan.LoanerId,
                InventoryId = loan.InventoryId,
                InventoryStatus = loan.Inventory?.Status
            };
        }

        private static string GetFriendlyDatabaseError(string message)
        {
            if (message.Contains("Copy does not exist"))
                return "The selected inventory copy does not exist.";

            if (message.Contains("Copy is not available"))
                return "The selected inventory copy is not available.";

            if (message.Contains("maximum"))
                return "The loaner has reached the maximum number of active or overdue loans.";

            if (message.Contains("Loan not found"))
                return "The loan was not found.";

            if (message.Contains("Loan already returned"))
                return "The loan has already been returned.";

            return message;
        }
    }
}
