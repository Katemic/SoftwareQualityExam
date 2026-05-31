using LibraryAPI.DTOs;
using LibraryAPI.Services;
using LibraryAPI.Services.Interfaces;
using LibrarySQLBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Serialization;
using System.Security.Claims;

namespace LibraryAPI.Controllers
{
    public enum ReservationStatus
    {
        [EnumMember(Value = "pending")]
        Pending,
        [EnumMember(Value = "ready for pickup")]
        ReadyForPickup,
        [EnumMember(Value = "fulfilled")]
        Fulfilled
    }
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
            try { 
            var reservations = await _reservationService.GetAllReservations();
            return Ok(reservations);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpGet("MyReservations")]
        public async Task<IActionResult> GetAllLoanersReservations()
        {
            try { 
            var id = int.Parse(
             User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reservations = await _reservationService.GetAllLoanersReservation(id);
            if(reservations == null || reservations.Count == 0)
            {
                return NotFound();
            }
            return Ok(reservations);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateReservation(CreateReservationDto dto)
        {

            try
            {
                var loanerId = int.Parse(
             User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var created = await _reservationService.CreateReservation(dto, loanerId);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("Update/{id}")] 
        public async Task<IActionResult> UpdateReservation(int id, ReservationStatus status) 
        {
            try { 
            var updatedReservation = await _reservationService.UpdateReservation(id, status);

            if (updatedReservation == null)
            {
                return NotFound();
            }

            return Ok(updatedReservation);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteReservation(int itemId)
        {
            try { 
            var loanerId = int.Parse(
             User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var deleted = await _reservationService.DeleteReservation(itemId, loanerId);

            if (!deleted)
            {
                return NotFound();
            }

            return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
