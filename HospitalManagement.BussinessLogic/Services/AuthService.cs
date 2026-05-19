using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
// 🔥 تأكدي من استدعاء مسار الـ DbContext الخاص بمشروعك هنا، على سبيل المثال:
// using HospitalManagement.DataAccess.Data; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        // 🔥 حقن الـ DbContext للتعامل المباشر والسريع مع الجداول الفرعية
        private readonly AppDbContext _context;

        // قم بتحديث الـ Constructor لاستقبال الـ DbContext (استبدلي AppDbContext باسم السياق لديكِ إن كان مختلفاً)
        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailService emailService, AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
            _context = context;
        }

        public async Task<bool> RequestOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email };
                await _userManager.CreateAsync(user);
            }

            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            string body = $@"
            <div style='font-family: Arial; text-align: center;'>
                <h2>Welcome to Pulse</h2>
                <p>Your secure access code is:</p>
                <h1 style='color: #088395;'>{code}</h1>
                <p>This code will expire in 5 minutes.</p>
            </div>";

            await _emailService.SendEmailAsync(email, "Pulse Access Code", body);

            return true;
        }

        public async Task<AuthModel> VerifyOtpAsync(string email, string code)
        {
            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                authModel.Message = "User not found.";
                return authModel;
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", code);

            if (isValid)
            {
                var jwtToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // ربط التوكن بالـ UserId الخاص بالمستخدم
                refreshToken.UserId = user.Id;

                //  إضافة التوكن الجديد للجدول الفرعي مباشرة عبر الـ Context
                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync();

                authModel.IsAuthenticated = true;
                authModel.Token = jwtToken;
                authModel.RefreshToken = refreshToken.Token;

                return authModel;
            }

            authModel.Message = "Invalid OTP code.";
            return authModel;
        }

        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var authModel = new AuthModel();

            // 1. جلب التوكن من الجدول مباشرة مع بيانات المستخدم الخاص به عبر الـ Include لتوفير أداء أسرع بكثير
            var refreshToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (refreshToken == null)
            {
                authModel.Message = "Invalid token.";
                return authModel;
            }

            // 2. معالجة وحماية هجمات التصادم المتزامن (Concurrency/Race Condition) في الـ الفرونت إند
            if (!refreshToken.IsActive)
            {
                // مصيدة الـ 5 ثوانٍ الذكية: إذا تم إلغاء هذا التوكن قبل أقل من 5 ثوانٍ (بسبب طلب متزامن أطلقه الـ Interceptor)
                // نعتبر الطلب ناجحاً ونمرر له أحدث توكن نشط متولد في قاعدة البيانات بدلاً من طرده تعسفياً
                if (refreshToken.RevokedOn >= DateTime.UtcNow.AddSeconds(-5))
                {
                    var latestToken = await _context.RefreshTokens
                        .Where(t => t.UserId == refreshToken.UserId)
                        .OrderByDescending(t => t.CreatedOn)
                        .FirstOrDefaultAsync();

                    if (latestToken != null)
                    {
                        authModel.IsAuthenticated = true;
                        authModel.Token = GenerateJwtToken(refreshToken.User);
                        authModel.RefreshToken = latestToken.Token;
                        return authModel;
                    }
                }

                authModel.Message = "Inactive token.";
                return authModel;
            }

            // 3. إلغاء صلاحية التوكن القديم فوراً في نفس اللحظة لمنع إعادة استخدامه
            refreshToken.RevokedOn = DateTime.UtcNow;

            // 4. توليد التوكنات الجديدة
            var newJwtToken = GenerateJwtToken(refreshToken.User);
            var newRefreshToken = GenerateRefreshToken();
            newRefreshToken.UserId = refreshToken.UserId; // ربطه بنفس المستخدم

            // 5.  تحديث وإضافة التوكنات بطلب خفيف وسريع جداً على جدول الـ RefreshTokens فقط
            _context.RefreshTokens.Add(newRefreshToken);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // حماية أخيرة في حال حدوث تصادم لحظي على مستوى الداتابيز، نمرر التوكن الجديد المتولد بالذاكرة لإنقاذ الجلسة
                authModel.IsAuthenticated = true;
                authModel.Token = newJwtToken;
                authModel.RefreshToken = newRefreshToken.Token;
                return authModel;
            }

            authModel.IsAuthenticated = true;
            authModel.Token = newJwtToken;
            authModel.RefreshToken = newRefreshToken.Token;

            return authModel;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
                return false;

            // إلغاء تفعيل السطر في الجدول وحفظ التغيير مباشرة
            refreshToken.RevokedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}