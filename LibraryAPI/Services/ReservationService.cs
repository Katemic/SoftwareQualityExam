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
