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
        void AddDoctor(AddDoctorVM doctorVM);
        IEnumerable<Doctor> GetAllDoctors();
        Doctor GetDoctorById(int id);
        IEnumerable<Doctor> GetDoctorsByDepartment(int departmentID);
        void DeleteDoctor(int Id);
        void UpdateDoctor(DoctorVM doctor);
    }
}
