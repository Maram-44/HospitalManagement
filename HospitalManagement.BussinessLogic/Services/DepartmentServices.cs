using AutoMapper;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Entities;
using HospitalManagement.DataAccess.Repositories.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    public class DepartmentServices : IDepartmentServices
    {
        private readonly IGenericRepository<Department> _repository;
        private readonly IMapper _mapper;

        public DepartmentServices(IGenericRepository<Department> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<DepartmentDTO> GetDepartmentById(int departmentId)
        {
            Department department =await _repository.FindByIdAsync(departmentId);
            return _mapper.Map<DepartmentDTO>(department);
        }

        public async Task<IEnumerable<DepartmentDTO>> GetDepartments()
        {
           var departments =  _repository.FindAll();
            return _mapper.Map<IEnumerable<DepartmentDTO>>(departments);
        }
    }
}
