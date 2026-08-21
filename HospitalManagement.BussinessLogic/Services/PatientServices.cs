using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.Mappers;
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
    internal class PatientServices : IpatientServices
    {
        private readonly AppDbContext _context;
        public PatientServices(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Patient> CreateOrUpdatePatientAsync(PatientDTO patientDTO)
        {
            // البحث عن المريض بناءً على نوع ورقم الهوية
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.IdNumber == patientDTO.IdNumber && p.IdType == patientDTO.IdType);

            try
            {
                if (patient == null)
                {
                    // حالة مريض جديد: التحويل لـ Entity والإضافة
                    patient = patientDTO.ToEntity();
                    _context.Patients.Add(patient);
                }
                else
                {
                    // حالة مريض مسجل مسبقاً: تحديث الكائن مباشرة باستخدام المابير
                    patient.UpdateFromDto(patientDTO);
                    _context.Patients.Update(patient);
                }

                await _context.SaveChangesAsync();
                return patient; // نعيد المريض لنحصل على الـ ID الخاص به
            }
            catch (Exception)
            {
                throw; // نترك التعامل مع الخطأ للخدمة الرئيسية (AppointmentService)
            }
        }

        // دالة البحث لتعبئة الفورم تلقائياً في React
        public async Task<PatientDTO> GetPatientByIDNumberAndIdType(string IDNumber, string IDType)
        {

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.IdNumber == IDNumber && p.IdType == IDType);

            if (patient == null) return null;

            return patient.ToDto();
        }
    }
}
