using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using LibrarySQLBackend.Repositories;
using LibrarySQLBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ReservationDto> CreateReservation(CreateReservationDto createReservationDto)
        {
            if (!await _reservationRepository.ItemExistsAsync(createReservationDto.ItemId))
            {
                throw new KeyNotFoundException("Item not found.");
            }

            if (!await _reservationRepository.LoanerExistsAsync(createReservationDto.LoanerId))
            {
                throw new KeyNotFoundException("Loaner not found.");
            }

            // Get next queue number
            var nextQueueNumber = (await _reservationRepository.GetByItemIdAsync(createReservationDto.ItemId))?
                .Count + 1 ?? 1;

            if(nextQueueNumber > 100)
            {
                throw new InvalidOperationException("Reservation queue is full.");
            }

            var reservation = new Reservation
            {
                LoanerId = createReservationDto.LoanerId,
                ItemId = createReservationDto.ItemId,
                Status = "pending",
                QueueNumber = nextQueueNumber
            };
            await _reservationRepository.CreateReservationAsync(reservation);

            return MapToDto(reservation);
        }
        public async Task<ReservationDto?> UpdateReservation(int id,ReservationDto dto)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                return null;
            }

            reservation.Status = dto.Status;

            await _reservationRepository.UpdateAsync(reservation);

            return MapToDto(reservation);
        }
        public async Task<bool> DeleteReservation(int id)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                return false;
            }

            await _reservationRepository.DeleteAsync(reservation);
            List<Reservation>? remainingReservations = await _reservationRepository.GetByItemIdAsync(reservation.ItemId); // Update queue numbers for remaining reservations
            if (remainingReservations != null)
            {
                foreach (var remainingReservation in remainingReservations)
                {
                    remainingReservation.QueueNumber--;
                    await _reservationRepository.UpdateAsync(remainingReservation);
                }
            }

            return true;
        }
        private ReservationDto MapToDto(Reservation reservation)
        {
            ReservationDto reservationDto = new ReservationDto
            {
                ItemId = reservation.ItemId,
                LoanerId = reservation.LoanerId,
                Status = reservation.Status,
                queue_number = reservation.QueueNumber
            };
            return reservationDto;
        }
    }
}
