using LibraryAPI.DTOs;

namespace LibraryAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservation(CreateReservationDto createReservationDto);

    }
}
