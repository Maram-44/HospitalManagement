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
    public class DoctorServices : IDoctorServices
    {
        private readonly IGenericRepository<Doctor> _repository;
        private readonly IMapper _mapper;
        public DoctorServices(IGenericRepository<Doctor> repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }
        public void AddDoctor(AddDoctorVM doctorVM)
        {
            var doctor=_mapper.Map<Doctor>(doctorVM);
            _repository.AddOne(doctor);
        }

        public void DeleteDoctor(int Id)
        {
            _repository.FindById(Id);
        }

        public IEnumerable<Doctor> GetAllDoctors()
        {
            var doctors=_repository.FindAll();
            if (doctors == null)
                return null;

            return doctors;
        }

        public Doctor GetDoctorById(int id)
        {
            var doctor=_repository.FindById(id);
            if (doctor == null)
                return null;

            return doctor;
        }

        public IEnumerable<Doctor> GetDoctorsByDepartment(int departmentID)
        {
            var doctors=_repository.FindAll().Where(d=>d.DepartmentId==departmentID);
            if (doctors == null)
                return null;

            return doctors;
        }

        public void UpdateDoctor(DoctorVM doctorVM)
        {
            var doctor = _mapper.Map<Doctor>(doctorVM);
            _repository.UpdateOne(doctor);
        }
    }
}