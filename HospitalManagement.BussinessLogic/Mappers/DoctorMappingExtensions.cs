using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Mappers
{
    public static class DoctorMappingExtensions
    {
        public static DoctorDTO ToDto(this Doctor doctor, List<DoctorLeave> doctorLeaves)
        {
            if (doctor == null) return null;

            (string nextDayAr, string nextDayEn) = GetNextAvailableDayDescriptionInMem(doctorLeaves);

            return new DoctorDTO
            {
                Id = doctor.Id,
                // الأسماء باللغة العربية
                FirstNameAr = doctor.FirstNameAr,
                SecondNameAr = doctor.SecondNameAr,
                ThirdNameAr = doctor.ThirdNameAr,
                LastNameAr = doctor.LastNameAr,

                // الأسماء باللغة الإنجليزية
                FirstNameEn = doctor.FirstName,
                SecoundNameEn = doctor.SecoundName,
                ThirdNameEn = doctor.ThirdName,
                LastNameEn = doctor.LastName,

                // التخصص والنوع والأسعار
                SpecialtyAr = doctor.SpecialtyAr,
                SpecialtyEn = doctor.Specialty,
                Price = doctor.Price,
                GenderAr = doctor.GenderAr,
                GenderEn = doctor.Gender,

                // البيانات الإضافية والعلاقات
                DepartmentId = doctor.DepartmentId,
                ImagePath = doctor.ImagePath,
                BranchEn = doctor.branch?.BranchName ?? "No Branch", // حماية في حال كان الفرع Null
                BranchAr = doctor.branch?.BranchNameAr ?? "No Branch",

                // الحقل الديناميكي القادم من قاعدة البيانات
                NextAvailableDayEn = nextDayEn,
                NextAvailableDayAr = nextDayAr,

            };
        }

        public static (string Ar, string En) GetNextAvailableDayDescriptionInMem(List<DoctorLeave> doctorLeaves)
        {
            DateOnly candidateDate = DateOnly.FromDateTime(DateTime.Today);
            int maxSearchDays = 30;
            int iterations = 0;
            TimeSpan eveningEnd = new TimeSpan(22, 0, 0);
            DateOnly? foundDate = null;

            while (iterations < maxSearchDays)
            {
                if (candidateDate == DateOnly.FromDateTime(DateTime.Today))
                {
                    if (DateTime.Now.TimeOfDay >= eveningEnd)
                    {
                        candidateDate = candidateDate.AddDays(1);
                        continue;
                    }
                }

                if (candidateDate.DayOfWeek == DayOfWeek.Friday)
                {
                    candidateDate = candidateDate.AddDays(1);
                    iterations++;
                    continue;
                }

                bool isOnLeave = doctorLeaves.Any(l => candidateDate >= l.StartDate && candidateDate <= l.EndDate);
                if (isOnLeave)
                {
                    candidateDate = candidateDate.AddDays(1);
                    iterations++;
                    continue;
                }

                foundDate = candidateDate;
                break;
            }

            if (foundDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                if (foundDate.Value == today)
                    return ("اليوم", "Today");

                if (foundDate.Value == today.AddDays(1))
                    return ("غداً", "Tomorrow");

                string arDay = foundDate.Value.ToDateTime(TimeOnly.MinValue).ToString("dddd yyyy-MM-dd", new CultureInfo("ar-SA"));
                string enDay = foundDate.Value.ToDateTime(TimeOnly.MinValue).ToString("dddd yyyy-MM-dd", new CultureInfo("en-US"));

                return (arDay, enDay);
            }

            return ("لا توجد مواعيد متاحة", "No dates are currently available.");
        }
    }
}
