using AutoMapper;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Tools
{
    public class Mapping : Profile
    {
        public Mapping() 
        {
            CreateMap<PatientVM, Patient>();
            CreateMap<DoctorVM, Doctor>();
            CreateMap<DepartmentVM, Department>();
            CreateMap<AppoimentVM,Appointment>();
            CreateMap<AddPatientMV, Patient>();
            CreateMap<AddDoctorVM,Doctor>();
        }
    }
}
