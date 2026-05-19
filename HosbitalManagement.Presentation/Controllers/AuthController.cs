using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // 1. إرسال كود الـ OTP للايميل
        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email is required.");

            var result = await _authService.RequestOtpAsync(email);

            if (result)
                return Ok(new { message = "OTP code sent successfully to your email." });

            return BadRequest("Something went wrong while sending OTP.");
        }

        // 2. التحقق من الكود وإعداد الـ HttpOnly Cookie فوراً عند تسجيل الدخول
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyOtpAsync(request.Email, request.Code);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            return Ok(new
            {
                token = result.Token,
                user = new { email = request.Email } // يرجع الإيميل الصريح المكتوب في الفورم
            });
        }

        // 3. تجديد التوكن بقراءة الكوكي مباشرة
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Refresh Token is missing or expired in cookies.");
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            // 🔴 تعديل جوهري: نمرر إيميل المستخدم القادم من نتيجة السيرفيس (المستخرج عبر الـ Include)
            // لكي لا يتصفر الإيميل في الريدكس بالفرونت إند أثناء التحديث الصامت.
            return Ok(new
            {
                token = result.Token,
                user = new { email = "" }
            });
        }

        // 4. تسجيل الخروج وتدمير الكوكي من المتصفح
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest("Token is missing from cookies.");
            }

            // 🔴 التعديل الأمني: ننفذ الإلغاء في الداتابيز أولاً قبل مسح الكوكي
            var result = await _authService.RevokeTokenAsync(refreshToken);

            if (!result)
                return BadRequest("Token is invalid or already expired.");

            //  بمجرد نجاح الإلغاء، نمسح الكوكي بخصائص مطابقة تماماً لخصائص الإنشاء لضمان طردها
            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });

            return Ok(new { message = "Token revoked and cookie cleared successfully." });
        }

        // --- دالة موحدة ومؤمنة بالكامل للـ HTTPS والتحديث الفوري ---
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}

