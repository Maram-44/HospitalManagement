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
    public interface IDoctorServices
    {
        Task<IEnumerable<DoctorDTO>> GetAllDoctors();
        Task<DoctorDTO> GetDoctorById(int id);
        Task<IEnumerable<DoctorDTO>> GetDoctorsByDepartment(int departmentID);
        Task<bool> IsDoctorAvailable(int doctorId, DateOnly requestedDate);
        Task<IEnumerable<WorkingDayDTO>> GetNextSevenAvailableDays(int doctorId);
    }
}
