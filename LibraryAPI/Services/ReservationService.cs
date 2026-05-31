using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Reflection;
using System.Runtime.Serialization;

namespace LibraryAPI.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;

        public ReservationService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }
        public async Task<List<ReservationDto>> GetAllReservations()
        {
            var reservations = await _reservationRepository.GetAllAsync();
            return reservations.Select(MapToDto).ToList();
        }
        public async Task<List<ReservationDto>> GetAllLoanersReservation(int loanerId)
        {
            var activeLoans = await _reservationRepository.GetByLoanerId(loanerId);
            return activeLoans.Select(MapToDto).ToList();
        }
        public async Task<ReservationDto> CreateReservation(CreateReservationDto createReservationDto, int loanerId)
        {
            var nextQueueNumber = await ValidateData(createReservationDto, loanerId);
            if(!await _reservationRepository.ItemIsUnavailable(createReservationDto.ItemId))
            {
                throw new InvalidOperationException("Item is currently available for loan.");
            }   
            var reservation = new Reservation
            {
                LoanerId = loanerId,
                ItemId = createReservationDto.ItemId,
                Status = "pending",
                QueueNumber = nextQueueNumber
            };
            await _reservationRepository.CreateReservationAsync(reservation);

            return MapToDto(reservation);
        }
        // For updating status
        public async Task<ReservationDto?> UpdateReservation(int id,Enum status)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                return null;
            }

            reservation.Status = GetEnumMemberValue(status.ToString());

            await _reservationRepository.UpdateAsync(reservation);

            return MapToDto(reservation);
        }
        // Delete a reservation and update queue numbers for remaining reservations of the same item
        public async Task<bool> DeleteReservation(int itemId, int loanerId)
        {
            var reservations = await _reservationRepository.GetByLoanerId(loanerId);

            var reservation = reservations
                .FirstOrDefault(r => r.ItemId == itemId);

            if (reservation == null)
            {
                throw new KeyNotFoundException(
                    "Reservation not found for this loaner.");
            }

            await _reservationRepository.DeleteAsync(reservation);

            var remainingReservations =
                await _reservationRepository.GetByItemIdAsync(itemId);

            foreach (var remainingReservation in remainingReservations)
            {
                if (remainingReservation.QueueNumber > reservation.QueueNumber)
                {
                    remainingReservation.QueueNumber--;

                    await _reservationRepository.UpdateAsync(
                        remainingReservation);
                }
            }

            return true;
        }
        private ReservationDto MapToDto(Reservation reservation)
        {
            ReservationDto reservationDto = new ReservationDto
            {
                Id = reservation.Id,
                ItemId = reservation.ItemId,
                LoanerId = reservation.LoanerId,
                Status = reservation.Status,
                queue_number = reservation.QueueNumber
            };
            return reservationDto;
        }
       
        private async Task<int> ValidateData(CreateReservationDto createReservationDto, int loanerId)
        {
            if (!await _reservationRepository.ItemExistsAsync(createReservationDto.ItemId))
            {
                throw new KeyNotFoundException("Item not found.");
            }

            if (!await _reservationRepository.LoanerExistsAsync(loanerId))
            {
                throw new KeyNotFoundException("Loaner not found.");
            }
            var loanerReservations = await _reservationRepository.GetByLoanerId(loanerId);
            if (loanerReservations.Count >= 3)
            {
                throw new InvalidOperationException("Loaner has reached the maximum number of active reservations.");
            }
            var existingReservations = await _reservationRepository.GetByItemIdAsync(createReservationDto.ItemId);

            // Get next queue number
            var nextQueueNumber = existingReservations?.Count + 1 ?? 1;

            if(existingReservations.Contains(existingReservations.FirstOrDefault(r => r.QueueNumber == nextQueueNumber)))
            {
                throw new InvalidOperationException("Queue number already exists for this item.");
            }

            if (nextQueueNumber > 100)
            {
                throw new InvalidOperationException("Reservation queue is full.");
            }

            if (existingReservations.Contains(existingReservations.FirstOrDefault(r => r.LoanerId == loanerId)))
            {
                throw new InvalidOperationException("Loaner has already reserved this item.");
            }

            return nextQueueNumber;
        }

        // Helps with status Enums
        public string GetEnumMemberValue(string correctEnum)
        {
            if(correctEnum == null)
            {
                throw new KeyNotFoundException("Enum was left as null");
            }
            if (correctEnum == "ReadyForPickup") {
                return "ready for pickup"; 
            }
            if (correctEnum == "Pending")
            {
                return "pending";
            }
            if (correctEnum == "Fulfilled")
            {
                return "fulfilled";
            }
            throw new KeyNotFoundException("This is not a valid value");
        }
    }
}
