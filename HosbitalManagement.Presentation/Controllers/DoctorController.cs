using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorServices _services;
        public DoctorController(IDoctorServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Route("")]
        public ActionResult CreateDoctor(AddDoctorVM doctor)
        {
            if (ModelState.IsValid)
            {
                _services.AddDoctor(doctor);
                return Ok();
            }
            return BadRequest();    
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult GetDoctorById(int id)
        {
            var doctor= _services.GetDoctorById(id);
            if(doctor == null)
            {
                return NotFound();
            }
            return Ok(doctor);
        }

        [HttpGet]
        [Route("")]
        public ActionResult GetAllDoctors()
        {
            var doctors= _services.GetAllDoctors();
            return Ok(doctors);
        }

        [HttpGet]
        [Route("department/{departmentId}")]
        public ActionResult GetDoctorsByDepartment(int departmentId) 
        {
            var doctors=_services.GetDoctorsByDepartment(departmentId);
            if (doctors == null)
                return NotFound();

            return Ok(doctors);
        }

        [HttpDelete]
        [Route("Delete/{id}")]
        public ActionResult DeleteDoctor(int id)
        {
            var doctor=_services.GetDoctorById(id);
            if(doctor == null)
                return NotFound();
            _services.DeleteDoctor(id);
            return Ok();

        }

        [HttpPut]
        [Route("")]
        public ActionResult UpdateDoctor(DoctorVM doctorVM)
        {
            var doctor=_services.GetDoctorById(doctorVM.Id);

            if(doctor == null)
                return NotFound();

            _services.UpdateDoctor(doctorVM);

            return Ok();
        }
    }
}
