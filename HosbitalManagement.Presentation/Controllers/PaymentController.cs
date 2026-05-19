using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using System.Security.Claims;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentController : ControllerBase
    {
        private readonly IAppoimentServices _appointmentService;
        private readonly StripeSettings _stripeSettings;

        // نقوم بحقن IOptions<StripeSettings> في المشيد
        public PaymentController(IAppoimentServices appointmentService, IOptions<StripeSettings> stripeSettings)
        {
            _appointmentService = appointmentService;
            _stripeSettings = stripeSettings.Value;
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequestDTO request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found.");

            decimal expectedPrice = await _appointmentService.GetExpectedPrice(
                request.DoctorId,
                request.PatientId,
                request.AppointmentDate.ToDateTime(TimeOnly.MinValue));

            long amountInHalalas = (long)(expectedPrice * 100);
            if (amountInHalalas == 0) return Ok(new { clientSecret = "FREE_REVIEW" });

            // هنا السحر: نمرر المفتاح السري الخاص بكِ من الإعدادات لكل طلب
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInHalalas,
                Currency = "sar",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
            {
                {"From", "Plus" },
                { "DoctorId", request.DoctorId.ToString() },
                { "PatientId", request.PatientId?.ToString() ?? "New" },
                { "UserId", userId }
            }
            };

            var service = new PaymentIntentService();

            // نستخدم الـ SecretKey المستخرج من الـ Options Pattern
            var requestOptions = new RequestOptions { ApiKey = _stripeSettings.SecretKey };
            var intent = await service.CreateAsync(options, requestOptions);

            return Ok(new { clientSecret = intent.ClientSecret });
        }
    }
 }

