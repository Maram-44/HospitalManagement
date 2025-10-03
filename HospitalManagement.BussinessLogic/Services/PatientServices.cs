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
    internal class PatientServices : IpatientServices
    {
        private readonly IGenericRepository<Patient> _repository;
        private readonly IMapper _mapper;

        public PatientServices(IGenericRepository<Patient> repo, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repo;
        }

        public void AddPatient(AddPatientMV p)
        {
            var patient = _mapper.Map<Patient>(p);
            _repository.AddOne(patient);
        }

        public void UpdatePatient(PatientVM p)
        {
            var ExistingPatient = _mapper.Map<Patient>(p);
            _repository.UpdateOne(ExistingPatient);
        }

        public void DeletePatient(int id)
        {
            var Patient=_repository.FindById(id);
            _repository.DeleteOne(Patient);
        }

        public IEnumerable<Patient> GetAllPatients()
        {
            var patients = _repository.FindAll();
            return patients;
        }

        public Patient GetPatient(int id)
        {
            var Patient= _repository.FindById(id);
            return Patient;
        }
    }
}
