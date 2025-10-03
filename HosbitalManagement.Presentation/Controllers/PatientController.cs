using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HosbitalManagement.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Roles ="Admin")]
    public class PatientController : Controller
    {
        private readonly IpatientServices _patientServices;

        public PatientController(IpatientServices patientServices)
        {
            _patientServices = patientServices;
        }

        [HttpPost]
        [Route("")]
        public ActionResult<int> CreatePatient(AddPatientMV patientVM)
        {
            _patientServices.AddPatient(patientVM);
            return Ok();
        }

        [HttpPut]
        [Route("")]
        public ActionResult UpdatePatient(PatientVM patientVM)
        {
            _patientServices.UpdatePatient(patientVM);
            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        public ActionResult DeletePatient(int id)
        {
            _patientServices.DeletePatient(id);
            return Ok();
        }

        [HttpGet]
        [Route("")]
        public ActionResult getAllPatient()
        {
            var Patients = _patientServices.GetAllPatients();
            if (Patients != null)
            {
                return Ok(Patients);
            }
            
            return NotFound();
        }

        [HttpGet]
        [Route("{id}")]
        public ActionResult GetPatient(int id)
        {
            var patient=_patientServices.GetPatient(id);
           
            if (patient != null)
                return Ok(patient);

            return NotFound();
        }
    }
}
