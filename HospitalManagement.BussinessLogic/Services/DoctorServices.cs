using AutoMapper;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HospitalManagement.BussinessLogic.DTOs;

namespace HospitalManagement.BussinessLogic.Services
{
    public class DoctorServices : IDoctorServices
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public DoctorServices(AppDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<DoctorDTO>> GetAllDoctors()
        {
            // 1. جلب البيانات الأساسية من الداتابيز (تأكد أنها انتهت تماماً)
            // 1. جلب قائمة الأطباء من قاعدة البيانات أولاً (بدون الوصف)
            var doctorsList = await _context.Doctors
                .Include(d => d.branch)
                .ToListAsync();

            // 2. معالجة البيانات وتحويلها لـ DTO مع استدعاء الدالة الـ Async
            var doctorDtos = new List<DoctorDTO>();

            foreach (var d in doctorsList)
            {
                doctorDtos.Add(new DoctorDTO
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Specialty = d.Specialty,
                    Price = d.Price,
                    DepartmentId = d.DepartmentId,
                    Branch = d.branch.BranchName,
                    // الآن يمكنك استخدام await هنا بشكل طبيعي
                    NextAvailableDay = await GetNextAvailableDayDescription(d.Id)
                });
            }

            return doctorDtos;
        }

        public async Task<DoctorDTO> GetDoctorById(int id)
        {
            // 1. جلب بيانات الطبيب مع العلاقات (الفرع والقسم)
            var doctor = await _context.Doctors
                .Include(d => d.branch)
                .FirstOrDefaultAsync(d => d.Id == id);

            // 2. حماية في حال لم يتم العثور على الطبيب
            if (doctor == null)
            {
                return null;
                // أو يمكنك رمي Exception مخصص: throw new KeyNotFoundException("Doctor not found");
            }

            // 3. استدعاء دالة وصف اليوم المتاح القادم (التي تسبب مشكلة الـ await عادةً)
            string nextAvailableDay = await GetNextAvailableDayDescription(doctor.Id);

            // 4. تحويل الكائن إلى DTO
            return new DoctorDTO
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialty = doctor.Specialty,
                Price = doctor.Price,
                Gender = doctor.gender, 
                DepartmentId = doctor.DepartmentId,
                Branch = doctor.branch.BranchName,
                NextAvailableDay = nextAvailableDay
            };
        }

        public async Task<IEnumerable<DoctorDTO>> GetDoctorsByDepartment(int departmentID)
        {
            // 1. جلب قائمة الأطباء من قاعدة البيانات أولاً (بدون الوصف)
            var doctorsList = await _context.Doctors
                .Where(d => d.DepartmentId == departmentID)
                .Include(d => d.branch)
                .ToListAsync();

            // 2. معالجة البيانات وتحويلها لـ DTO مع استدعاء الدالة الـ Async
            var doctorDtos = new List<DoctorDTO>();

            foreach (var d in doctorsList)
            {
                doctorDtos.Add(new DoctorDTO
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Specialty = d.Specialty,
                    Price = d.Price,
                    DepartmentId = d.DepartmentId,
                    Branch = d.branch.BranchName,
                    // الآن يمكنك استخدام await هنا بشكل طبيعي
                    NextAvailableDay = await GetNextAvailableDayDescription(d.Id)
                });
            }

            return doctorDtos;
        }
        public async Task<bool> IsDoctorAvailable(int doctorId, DateOnly requestedDate)
        {
            // التأكد من عدم وجود إجازة تغطي هذا التاريخ
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
            int maxSearchLimit = 30; // نبحث في نطاق شهرين كحد أقصى
            int iterations = 0;

            // جلب الإجازات مرة واحدة للأداء
            var leaves = await _context.DoctorLeaves
                .Where(l => l.DoctorId == doctorId && l.EndDate >= candidateDate)
                .AsNoTracking()
                .ToListAsync();

            while (availableDays.Count < 7 && iterations < maxSearchLimit)
            {
                // 1. تخطي الويكيند
                bool isWeekend = candidateDate.DayOfWeek == DayOfWeek.Friday || candidateDate.DayOfWeek == DayOfWeek.Saturday;

                // 2. التحقق من الإجازات
                bool isOnLeave = leaves.Any(l => candidateDate >= l.StartDate && candidateDate <= l.EndDate);

                if (!isWeekend && !isOnLeave)
                {
                        availableDays.Add(new WorkingDayDTO
                        {
                            // نرسل التاريخ بصيغة يوم/شهر للواجهة
                            Date = candidateDate.ToString("dd/MM"),
                            DayName = candidateDate.ToString("ddd"), // Mon, Tue...
                            FullDate = candidateDate.ToString("yyyy-MM-dd") // لإرساله عند الحجز
                        });
                }

                candidateDate = candidateDate.AddDays(1);
                iterations++;
            }

            return availableDays;
        }









        public async Task<string> GetNextAvailableDayDescription(int doctorId)
        {
            DateOnly candidateDate = DateOnly.FromDateTime(DateTime.Today);
            int maxSearchDays = 30;
            int iterations = 0;
            TimeSpan eveningEnd = new TimeSpan(22, 0, 0);

            var doctorLeaves = await _context.DoctorLeaves
                .Where(l => l.DoctorId == doctorId && l.EndDate >= candidateDate)
                .AsNoTracking()
                .ToListAsync();

            DateOnly? foundDate = null; // غيرناه لـ DateOnly ليتناسب مع candidateDate

            while (iterations < maxSearchDays)
            {
                // 1. التحقق من الوقت الحالي
                if (candidateDate == DateOnly.FromDateTime(DateTime.Today))
                {
                    if (DateTime.Now.TimeOfDay >= eveningEnd)
                    {
                        candidateDate = candidateDate.AddDays(1);
                        continue;
                    }
                }

                // 2. تخطي الويكند
                if (candidateDate.DayOfWeek == DayOfWeek.Friday || candidateDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    candidateDate = candidateDate.AddDays(1);
                    iterations++;
                    continue;
                }

                // 3. التحقق من الإجازات
                bool isOnLeave = doctorLeaves.Any(l => candidateDate >= l.StartDate && candidateDate <= l.EndDate);

                if (isOnLeave)
                {
                    candidateDate = candidateDate.AddDays(1);
                    iterations++;
                    continue;
                }

                // --- التعديل الجوهري هنا ---
                // إذا وصل الكود لهذه النقطة، فهذا يعني أن اليوم "متاح"
                foundDate = candidateDate;
                break; // نخرج من الحلقة فوراً لأننا وجدنا أول يوم متاح
            }

            // تنسيق النص المرجع
            if (foundDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                if (foundDate.Value == today)
                    return "Today";

                if (foundDate.Value == today.AddDays(1))
                    return "Tomorrow";

                // تحويل لـ DateTime فقط لأجل التنسيق (ToString)
                return foundDate.Value.ToDateTime(TimeOnly.MinValue).ToString("dddd yyyy-MM-dd");
            }

            return "No dates are currently available.";
        }

    }
}