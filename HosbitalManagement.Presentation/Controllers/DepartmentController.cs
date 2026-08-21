using HospitalManagement.BussinessLogic.Services.InterfacesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentServices _dpartmentServices;

        public DepartmentsController(IDepartmentServices dpartmentServices)
        {
            _dpartmentServices= dpartmentServices;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartments() 
        { 
            var departments= await _dpartmentServices.GetDepartments();
            if(departments==null)
                return NotFound();

            return Ok(departments);
        }


    }
}
