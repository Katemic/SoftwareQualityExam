using LibraryAPI.DTOs;
using System.Threading.Tasks;

namespace LibraryAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservation(CreateReservationDto createReservationDto);
        Task<List<ReservationDto>> GetAllReservations();
        Task<ReservationDto?> UpdateReservation(int id, ReservationDto dto);
        Task<bool> DeleteReservation(int id);
    }
}
