using System.Globalization;
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
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    public class DoctorServices : IDoctorServices
    {
        private readonly AppDbContext _context;



        //private readonly IWebHostEnvironment _env;

        public DoctorServices(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorDTO>> GetAllDoctors()
        {
            // 1. جلب قائمة الأطباء
            var doctorsList = await _context.Doctors
                .Include(d => d.branch)
                .AsNoTracking()
                .ToListAsync();

            var doctorDtos = new List<DoctorDTO>();

            // 2. جلب إجازات هؤلاء الأطباء فقط دفعة واحدة خارج الـ Loop
            var doctorIds = doctorsList.Select(d => d.Id).ToList();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            var allLeaves = await _context.DoctorLeaves
                .Where(l => doctorIds.Contains(l.DoctorId) && l.EndDate >= today)
                .AsNoTracking()
                .ToListAsync();

            // 3. المعالجة والتحويل المباشر باستخدام المابير الجديد
            foreach (var d in doctorsList)
            {
                var currentDoctorLeaves = allLeaves.Where(l => l.DoctorId == d.Id).ToList();

                // التعديل هنا: نمرر اللستة للمابير وهو يتكفل بالباقي داخلياً
                doctorDtos.Add(d.ToDto(currentDoctorLeaves));
            }

            return doctorDtos;
        }

        public async Task<DoctorDTO> GetDoctorById(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return null;

            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            // جلب إجازات هذا الطبيب فقط
            var doctorLeaves = await _context.DoctorLeaves
                .Where(l => l.DoctorId == id && l.EndDate >= today)
                .AsNoTracking()
                .ToListAsync();

            // التعديل هنا: استدعاء مباشر ونظيف للمابير
            return doctor.ToDto(doctorLeaves);
        }

        public async Task<IEnumerable<DoctorDTO>> GetDoctorsByDepartment(int departmentID)
        {
            // 1. جلب أطباء القسم
            var doctorsList = await _context.Doctors
                .Where(d => d.DepartmentId == departmentID)
                .Include(d => d.branch)
                .AsNoTracking()
                .ToListAsync();

            var doctorDtos = new List<DoctorDTO>();

            // 2. جلب الإجازات لأطباء القسم المحدد فقط في استعلام واحد
            var doctorIds = doctorsList.Select(d => d.Id).ToList();
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            var allLeaves = await _context.DoctorLeaves
                .Where(l => doctorIds.Contains(l.DoctorId) && l.EndDate >= today)
                .AsNoTracking()
                .ToListAsync();

            // 3. المعالجة والتحويل
            foreach (var d in doctorsList)
            {
                var currentDoctorLeaves = allLeaves.Where(l => l.DoctorId == d.Id).ToList();

                // التعديل هنا: استبدال اللوجيك القديم بالمابير الذكي
                doctorDtos.Add(d.ToDto(currentDoctorLeaves));
            }

            return doctorDtos;
        }

        public async Task<bool> IsDoctorAvailable(int doctorId, DateOnly requestedDate)
        {
            bool hasLeave = await _context.DoctorLeaves
                .AnyAsync(l => l.DoctorId == doctorId &&
                               requestedDate >= l.StartDate &&
                               requestedDate <= l.EndDate);

            return !hasLeave;
        }

        public async Task<IEnumerable<WorkingDayDTO>> GetNextSevenAvailableDays(int doctorId)
        {
            var availableDays = new List<WorkingDayDTO>();
            DateOnly candidateDate = DateOnly.FromDateTime(DateTime.Today);
            int maxSearchLimit = 30;
            int iterations = 0;

            var leaves = await _context.DoctorLeaves
                .Where(l => l.DoctorId == doctorId && l.EndDate >= candidateDate)
                .AsNoTracking()
                .ToListAsync();

            while (availableDays.Count < 7 && iterations < maxSearchLimit)
            {
                bool isWeekend = candidateDate.DayOfWeek == DayOfWeek.Friday;
                bool isOnLeave = leaves.Any(l => candidateDate >= l.StartDate && candidateDate <= l.EndDate);

                if (!isWeekend && !isOnLeave)
                {
                    availableDays.Add(new WorkingDayDTO
                    {
                        DayNameAr = candidateDate.ToDateTime(TimeOnly.MinValue).ToString("ddd", new CultureInfo("ar-SA")),
                        DayNameEn = candidateDate.ToDateTime(TimeOnly.MinValue).ToString("ddd", new CultureInfo("en-US")),
                        Date = candidateDate.ToString("dd/MM"),
                        FullDate = candidateDate.ToString("yyyy-MM-dd")
                    });
                }

                candidateDate = candidateDate.AddDays(1);
                iterations++;
            }

            return availableDays;
        }




        //private (string Ar, string En) GetNextAvailableDayDescriptionInMem(List<DoctorLeave> doctorLeaves)
        //{
        //    DateOnly candidateDate = DateOnly.FromDateTime(DateTime.Today);
        //    int maxSearchDays = 30;
        //    int iterations = 0;
        //    TimeSpan eveningEnd = new TimeSpan(22, 0, 0);

        //    DateOnly? foundDate = null;

        //    while (iterations < maxSearchDays)
        //    {
        //        if (candidateDate == DateOnly.FromDateTime(DateTime.Today))
        //        {
        //            if (DateTime.Now.TimeOfDay >= eveningEnd)
        //            {
        //                candidateDate = candidateDate.AddDays(1);
        //                continue;
        //            }
        //        }

        //        if (candidateDate.DayOfWeek == DayOfWeek.Friday)
        //        {
        //            candidateDate = candidateDate.AddDays(1);
        //            iterations++;
        //            continue;
        //        }

        //        // البحث يتم في القائمة الممررة مسبقاً (داخل الذاكرة)
        //        bool isOnLeave = doctorLeaves.Any(l => candidateDate >= l.StartDate && candidateDate <= l.EndDate);

        //        if (isOnLeave)
        //        {
        //            candidateDate = candidateDate.AddDays(1);
        //            iterations++;
        //            continue;
        //        }

        //        foundDate = candidateDate;
        //        break;
        //    }

        //    if (foundDate.HasValue)
        //    {
        //        var today = DateOnly.FromDateTime(DateTime.Today);

        //        if (foundDate.Value == today)
        //            return ("اليوم","Today");

        //        if (foundDate.Value == today.AddDays(1))
        //            return ("غداً", "Tomorrow");

        //        return (foundDate.Value.ToDateTime(TimeOnly.MinValue).ToString("dddd yyyy-MM-dd"), foundDate.Value.ToDateTime(TimeOnly.MinValue).ToString("dddd yyyy-MM-dd"));
        //    }

        //    return ("لا توجد مواعيد متاحة", "No dates are currently available.");
        //}

    }
}