using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibraryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _reservationService.GetAllReservations();
            return Ok(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan(CreateReservationDto dto)
        {
            try
            {
                var created = await _reservationService.CreateReservation(dto);

                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, ReservationDto dto)
        {
            var updatedReservation = await _reservationService.UpdateReservation(id, dto);

            if (updatedReservation == null)
            {
                return NotFound();
            }

            return Ok(updatedReservation);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var deleted = await _reservationService.DeleteReservation(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
