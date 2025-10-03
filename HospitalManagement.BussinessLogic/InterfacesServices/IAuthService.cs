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
        string AuthenticateRequest(AuthenticationRequest request);
    }
}
