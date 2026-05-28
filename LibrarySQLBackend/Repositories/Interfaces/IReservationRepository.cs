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
        Task<List<Reservation>?> GetByItemIdAsync(int itemId);
        Task<Reservation?> CreateReservationAsync(int loanerId, int itemId);
    }
}
