using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.InterfacesServices
{
    public interface IpatientServices
    {
        public void AddPatient(AddPatientMV p);
        public void UpdatePatient(PatientVM p);
        public void DeletePatient(int p);
        public IEnumerable<Patient> GetAllPatients();
        public Patient GetPatient(int id);
    }
}
