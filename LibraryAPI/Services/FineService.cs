using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories.Interfaces;

namespace LibraryAPI.Services
{
    public class FineService : IFineService
    {
        private readonly IFineRepository _fineRepository;
        private readonly ILoanRepository _loanRepository;

        public FineService(
            IFineRepository fineRepository,
            ILoanRepository loanRepository)
        {
            _fineRepository = fineRepository;
            _loanRepository = loanRepository;
        }

        public async Task<IEnumerable<FineDto>> GetAllAsync()
        {
            var fines = await _fineRepository.GetAllAsync();

            return fines.Select(MapToDto);
        }

        public async Task<FineDto?> GetByIdAsync(int id)
        {
            var fine = await _fineRepository.GetByIdAsync(id);

            return fine == null
                ? null
                : MapToDto(fine);
        }

        public async Task<IEnumerable<FineDto>> GetByLoanerIdAsync(int loanerId)
        {
            var fines = await _fineRepository.GetByLoanerIdAsync(loanerId);

            return fines.Select(MapToDto);
        }

        public async Task<FineDto> CreateAsync(CreateFineDto dto)
        {
            var loan = await _loanRepository.GetByIdAsync(dto.LoanId);

            if (loan == null)
                throw new InvalidOperationException("Loan not found.");

            if (dto.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Fine amount must be greater than 0.");
            }

            if (loan.Status != "overdue")
            {
                throw new InvalidOperationException(
                    "Loan is not overdue.");
            }

            var existingFines =
                await _fineRepository.GetByLoanIdAsync(dto.LoanId);

            if (existingFines.Any())
            {
                throw new InvalidOperationException(
                    "Loan already has a fine.");
            }

            var fine = new Fine
            {
                LoanId = dto.LoanId,
                Amount = dto.Amount,
                Status = "unpaid"
            };

            var createdFine =
                await _fineRepository.AddAsync(fine);

            return MapToDto(createdFine);
        }

        public async Task PayFineAsync(int fineId)
        {
            var fine = await _fineRepository.GetByIdAsync(fineId);

            if (fine == null)
            {
                throw new InvalidOperationException(
                    "Fine not found.");
            }

            if (fine.Status == "paid")
            {
                throw new InvalidOperationException(
                    "Fine is already paid.");
            }

            fine.Status = "paid";

            await _fineRepository.UpdateAsync(fine);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _fineRepository.DeleteAsync(id);
        }

        private static FineDto MapToDto(Fine fine)
        {
            return new FineDto
            {
                Id = fine.Id,
                Amount = fine.Amount,
                Status = fine.Status,
                CreatedDate = fine.CreatedDate,
                PaidDate = fine.PaidDate,
                LoanId = fine.LoanId
            };
        }
    }
}