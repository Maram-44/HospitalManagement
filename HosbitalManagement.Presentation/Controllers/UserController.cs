using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HosbitalManagement.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [Route("auth")]
        public ActionResult<string>AuthenticateUser(AuthenticationRequest request)
        {
            var response = _authService.AuthenticateRequest(request);
            if(response == null)
            {
                return Unauthorized();
            }
            return Ok(response);
        }
    }
}
