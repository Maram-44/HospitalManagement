using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IpatientServices _patientServices;

        public PatientController(IpatientServices patientServices)
        {
            _patientServices = patientServices;
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetPatient([FromQuery] string idNumber, [FromQuery] string idType)
        {
            // التحقق من المدخلات قبل إرهاق السيرفر
            if (string.IsNullOrEmpty(idNumber) || string.IsNullOrEmpty(idType))
                return BadRequest(new { message = "ID Number and Type are required" });

            var patient = await _patientServices.GetPatientByIDNumberAndIdType(idNumber, idType);

            if (patient == null)
                return NotFound(new { message = "Patient not found" });

            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrUpdate([FromBody] PatientDTO patientDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _patientServices.CreateOrUpdatePatientAsync(patientDTO);

                // أفضل ممارسة: إرجاع 201 عند النجاح
                return Ok(new
                {
                    success = true,
                    message = "Patient data processed successfully",
                    data = result
                });
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ (Logging) مهم هنا في المشاريع الكبيرة
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

    }
}
