using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.ModelView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.InterfacesServices
{
    public interface IAuthService
    {
        Task<bool> RequestOtpAsync(string email);
        Task<AuthModel> VerifyOtpAsync(string email, string code);
        Task<AuthModel> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
    }
}
