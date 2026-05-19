using HospitalManagement.BussinessLogic.InterfacesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentServices _dpartmentServices;

        public DepartmentController(IDepartmentServices dpartmentServices)
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

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetNameOfDepartmentById(int id)
        {
            var Department=await _dpartmentServices.GetDepartmentById(id);

            if(Department==null)
                return NotFound();

            return Ok(Department);
        }
    }
}
