using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.InterfacesServices
{
    public interface IDepartmentServices
    {
        Task<IEnumerable<DepartmentDTO>> GetDepartments();
        Task<DepartmentDTO> GetDepartmentById(int  departmentId);
    }
}
