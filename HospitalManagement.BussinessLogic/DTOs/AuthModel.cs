using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class AuthModel
    {
        public string? Message { get; set; }           // للرسائل مثل "الكود خطأ" أو "المستخدم غير موجود"
        public bool IsAuthenticated { get; set; }      // هل نجحت عملية تسجيل الدخول؟
        public string? Token { get; set; }             // الـ JWT Token (Access Token)
        public string? RefreshToken { get; set; }      // الـ Refresh Token الجديد
        public DateTime RefreshTokenExpiration { get; set; } // تاريخ انتهاء الريفريش توكن
    }
}
