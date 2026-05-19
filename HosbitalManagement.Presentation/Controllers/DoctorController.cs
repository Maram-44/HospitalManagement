using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.BussinessLogic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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


        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult> GetDoctorById(int id)
        {
            var doctor= await _services.GetDoctorById(id);
            if(doctor == null)
            {
                return NotFound();
            }
            return Ok(doctor);
        }

        [HttpGet]
        public async Task<ActionResult> GetAllDoctors()
        {
            var doctors = await _services.GetAllDoctors();
            return Ok(doctors);
        }

        
        [HttpGet]
        [Route("department/{departmentId}")]
        public async Task<ActionResult> GetDoctorsByDepartment(int departmentId) 
        {
            var doctors= await _services.GetDoctorsByDepartment(departmentId);
            if (doctors == null)
                return NotFound();

            return Ok(doctors);
        }

        [HttpGet("{id}/available-days")]
        public async Task<IActionResult> GetAvailableDays(int id)
        {
            // استدعاء الخدمة التي كتبناها لجلب الـ 7 أيام القادمة
            var availableDays = await _services.GetNextSevenAvailableDays(id);

            if (availableDays == null || !availableDays.Any())
            {
                return NotFound("لا توجد مواعيد متاحة لهذا الطبيب حالياً.");
            }

            return Ok(availableDays);
        }

    }
}
