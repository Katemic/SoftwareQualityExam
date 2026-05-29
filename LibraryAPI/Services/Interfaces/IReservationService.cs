using LibraryAPI.DTOs;
using System.Threading.Tasks;

namespace LibraryAPI.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationDto> CreateReservation(CreateReservationDto createReservationDto);
        Task<List<ReservationDto>> GetAllReservations();
        Task<List<ReservationDto>> GetAllLoanersReservation(int loanerId);
        Task<ReservationDto?> UpdateReservation(int id, Enum status);
        Task<bool> DeleteReservation(int id);
    }
}
