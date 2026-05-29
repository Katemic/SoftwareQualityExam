using LibrarySQLBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibrarySQLBackend.Repositories.Interfaces
{
    public interface IReservationRepository
    {
        Task<List<Reservation>?> GetAllAsync();
        Task<Reservation?> GetByIdAsync(int id);
        Task<List<Reservation>?> GetByItemIdAsync(int itemId);
        Task<Reservation?> CreateReservationAsync(Reservation reservation);
        Task UpdateAsync(Reservation reservation);
        Task DeleteAsync(Reservation reservation);

        Task<bool> ItemExistsAsync(int itemId);
        Task<bool> LoanerExistsAsync(int loanerId);
    }
}
