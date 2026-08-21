using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Mappers
{
    public static class PatientMappingExtensions
    {
        // 1. تحويل من Entity إلى DTO (للقراءة والعرض)
        public static PatientDTO ToDto(this Patient patient)
        {
            if (patient == null) return null;

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

        // 2. تحويل من DTO إلى Entity (للإنشاء الجديد)
        public static Patient ToEntity(this PatientDTO dto)
        {
            if (dto == null) return null;

            return new Patient
            {
                FullName = dto.FullName,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Phone = dto.Phone,
                Email = dto.Email,
                IdNumber = dto.IdNumber,
                IdType = dto.IdType,
                Nationality = dto.Nationality ?? "Saudi" // القيمة الافتراضية
            };
        }

        // 3. تحديث الكائن الحالي ببيانات الـ DTO الجديدة (لحالة التعديل)
        public static void UpdateFromDto(this Patient patient, PatientDTO dto)
        {
            if (patient == null || dto == null) return;

            patient.FullName = dto.FullName;
            patient.Gender = dto.Gender;
            patient.DateOfBirth = dto.DateOfBirth;
            patient.Phone = dto.Phone;
            patient.Email = dto.Email;
            // حقول الهوية والجنسية غالباً لا تتغير في التحديث، لذا نكتفي بتحديث البيانات الشخصية
        }
    }
}
