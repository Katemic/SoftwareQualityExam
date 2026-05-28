using LibraryAPI.DTOs;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Repositories;
using LibrarySQLBackend.Repositories.Interfaces;

namespace LibraryAPI.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;

        public ReservationService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public Task<ReservationDto> CreateReservation(CreateReservationDto createReservationDto)
        {
            var reservation = _reservationRepository.CreateReservationAsync(createReservationDto.LoanerId,createReservationDto.ItemId);
            ReservationDto reservationDto = new ReservationDto
            {
                ItemId = reservation.Result.ItemId,
                LoanerId = reservation.Result.LoanerId,
                Status = reservation.Result.Status,
                queue_number = reservation.Result.QueueNumber
            };
            // Validate data

            return Task.FromResult(reservationDto);

        }

    }
}
