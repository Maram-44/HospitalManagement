using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppoimentServices _appointmentServices;

        public AppointmentController(IAppoimentServices appointmentServices)
        {
            _appointmentServices = appointmentServices;
        }

        [HttpPost("book")]
        public async Task<IActionResult> BookAppointment([FromBody] BookingRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // استخراج معرف المستخدم من الـ Token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            // استدعاء الخدمة
            var result = await _appointmentServices.ProcessBookingAfterPayment(request, userId);

            if (result != null)
            {
                return Ok(result);
            }

            return BadRequest(new { success = false, message = "Failed to book appointment. Doctor might be unavailable or a server error occurred." });
        }

        [HttpGet("MyAppointments")]
        public async Task<IActionResult> GetAppointmentsByUserID()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");

            var result = await _appointmentServices.GetAppointmentsByUserID(userId);

            if (result != null)
                return Ok(result);

            return NotFound();
        }

        [HttpPatch("cancel/{id}")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            try
            {
                // استدعاء الخدمة التي كتبناها
                var result = await _appointmentServices.CancelAppointmentAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "The appointment does not exist." });
                }

                return Ok(new { message = "The appointment was successfully cancelled and a refund was issued if applicable." });
            }
            catch (Exception ex)
            {
                // هنا نلتقط رسائل الـ Exception التي رميناها في الخدمة 
                // مثل "لا يمكن إلغاء الموعد قبل أقل من 12 ساعة"
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}