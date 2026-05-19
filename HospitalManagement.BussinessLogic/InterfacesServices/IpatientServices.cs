using HospitalManagement.BussinessLogic.DTOs;
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
        Task<PatientDTO> GetPatientByIDNumberAndIdType(string IDNumber, string IDType);
        Task<Patient> CreateOrUpdatePatientAsync(PatientDTO patientDTO);
    }
}
