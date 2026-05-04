using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;
using System.Data.Common;

namespace LibraryAPI.Services
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;

        public LoanService(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
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
            try
            {
                var newLoanId = await _loanRepository.CreateLoanAsync(dto.LoanerId, dto.InventoryId);

                var loan = await _loanRepository.GetByIdAsync(newLoanId);

                if (loan == null)
                    throw new InvalidOperationException("Loan was created, but could not be loaded afterwards.");

                return MapToDto(loan);
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException(GetFriendlyDatabaseError(ex.Message));
            }
        }

        public async Task ReturnLoanAsync(int loanId)
        {
            try
            {
                await _loanRepository.ReturnLoanAsync(loanId);
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException(GetFriendlyDatabaseError(ex.Message));
            }
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
