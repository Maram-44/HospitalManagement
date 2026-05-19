using AutoMapper;
using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using HospitalManagement.DataAccess.Repositories.IRepository;
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
                    // حالة مريض جديد: إنشاء سجل
                    patient = new Patient()
                    {
                        FullName = patientDTO.FullName,
                        Gender = patientDTO.Gender,
                        DateOfBirth = patientDTO.DateOfBirth,
                        Phone = patientDTO.Phone,
                        Email = patientDTO.Email,
                        IdNumber = patientDTO.IdNumber,
                        IdType = patientDTO.IdType,
                        Nationality=patientDTO.Nationality == null?"Saudi":patientDTO.Nationality,
                    };
                    _context.Patients.Add(patient);
                }
                else
                {
                    // حالة مريض مسجل مسبقاً: تحديث البيانات (لأن اليوزر قد يعدل عليها في الفورم)
                    patient.FullName = patientDTO.FullName;
                    patient.Gender = patientDTO.Gender;
                    patient.DateOfBirth = patientDTO.DateOfBirth;
                    patient.Phone = patientDTO.Phone;
                    patient.Email = patientDTO.Email;

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

            // تحويل الكيان إلى DTO (يمكنك استخدام AutoMapper هنا إذا كنت قد أعددته)
            return new PatientDTO
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Phone = patient.Phone,
                Email = patient.Email,
                IdNumber = patient.IdNumber,
                IdType = patient.IdType,
                Nationality = patient.Nationality
            };
        }
    }
}
