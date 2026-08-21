using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.BussinessLogic.Services.InterfacesServices;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    public class DepartmentServices : IDepartmentServices
    {
        private readonly AppDbContext _context;

        public DepartmentServices( AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DepartmentDTO>> GetDepartments()
        {
            // جلب البيانات وتحويلها مباشرة داخل استعلام قاعدة البيانات (أداء عالي جداً واختصار للكود)
            var departmentDtos = await _context.Departments
                .AsNoTracking()
                .Select(d => new DepartmentDTO
                {
                    Id = d.Id,
                    NameEn = d.Name,
                    NameAr = d.NameAr,
                    Image = d.Image,
                    DescriptionEn = d.Description,
                    DescriptionAr = d.DescriptionAr
                })
                .ToListAsync();

            return departmentDtos;
        }
    }
}
