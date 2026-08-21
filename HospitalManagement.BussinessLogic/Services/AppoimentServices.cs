using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.BussinessLogic.Services.InterfacesServices;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using HospitalManagement.DataAccess.Enums;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    internal class AppoimentServices : IAppoimentServices
    {
        private readonly AppDbContext _context;
        private readonly IpatientServices _patientServices;
        private readonly IDoctorServices _doctorServices;

        public AppoimentServices(AppDbContext context, IpatientServices patientServices, IDoctorServices doctorServices)
        {
            _context = context;
            _patientServices = patientServices;
            _doctorServices = doctorServices;
        }

        public async Task<decimal> GetExpectedPrice(int doctorId, int? patientId, DateTime appointmentDate)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return 0;

            if (patientId.HasValue)
            {
                var now = DateTime.Now;
                // 1. تحديد تاريخ البداية (تاريخ اليوم قبل 7 أيام)
                DateOnly sevenDaysAgo = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

                // 2. جلب آخر حجز بشرط أن يكون تاريخه ضمن الـ 7 أيام الماضية وحتى اليوم
                var lastAppointment = await _context.Appointments
                    .Where(a => a.PatientId == patientId &&
                                a.DoctorId == doctorId &&
                                a.IsReview == false &&
                                (a.Status == enAppoimentStatus.confirmed || a.Status == enAppoimentStatus.Completed) &&
                                a.Date >= sevenDaysAgo) // الفلترة الذكية من البداية لقاعدة البيانات
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.Time)
                    .FirstOrDefaultAsync();

                if (lastAppointment != null)
                {
                    // [تحديث تلقائي صامت للحالة الحالية]
                    DateTime fullOldAppointmentDateTime = lastAppointment.Date.ToDateTime(TimeOnly.FromTimeSpan(lastAppointment.Time));
                    var timePassedFromNow = now - fullOldAppointmentDateTime;
                    if (lastAppointment.Status == enAppoimentStatus.confirmed && timePassedFromNow.TotalSeconds > 0)
                    {
                        lastAppointment.Status = enAppoimentStatus.Completed;
                        await _context.SaveChangesAsync();
                    }

                    // الشرط الصحيح: يجب أن يكون الموعد القديم قد اكتمل (أو تم تحديثه تلقائياً لمكتمل)
                    if (lastAppointment.Status == enAppoimentStatus.Completed)
                    {
                            return 0; // مراجعة مجانية
                    }
                }
            }

            return doctor.Price; // حجز جديد بسعر كامل
        }
        public async Task<int?> ProcessBookingAfterPayment(BookingRequestDTO request, string userId)
        {
            // 1. التحقق من التوفر وجلب بيانات الطبيب
            var isAvailable = await _doctorServices.IsDoctorAvailable(request.DoctorId, request.Date);
            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (!isAvailable || doctor == null) return null;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. معالجة بيانات المريض
                var patient = await _patientServices.CreateOrUpdatePatientAsync(request.PatientInfo);

                // 3. نمرر تاريخ الزيارة الطبي المطلوب (request.Date) الفعلي لحساب السعر والمراجعة بدقة
                decimal finalPrice = await GetExpectedPrice(request.DoctorId, patient.Id, request.Date.ToDateTime(TimeOnly.MinValue));
                bool isReview = finalPrice == 0;

                // 4. التحقق من Stripe (فقط إذا لم تكن مراجعة)
                if (!isReview)
                {
                    var intent = await new PaymentIntentService().GetAsync(request.StripePaymentIntentId);
                    if (intent.Status != "succeeded" || intent.Amount != (long)(finalPrice * 100))
                        return null;
                }

                // 5. تحليل الوقت وإنشاء الموعد
                if (!DateTime.TryParseExact(
                    request.Time,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime parsedTime))
                {
                    return null;
                }

                var appointment = new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = request.DoctorId,
                    Date = request.Date, // تاريخ الزيارة المحجوز
                    Time = parsedTime.TimeOfDay, // وقت الزيارة المحجوز
                    Price = finalPrice,
                    IsReview = isReview,
                    StripePaymentIntentId = isReview ? "FREE_REVIEW" : request.StripePaymentIntentId,
                    Status = enAppoimentStatus.confirmed,
                    UserId = userId,
                    AppoimentDate = DateTime.Now // هذا تاريخ ووقت إنشاء الحجز بالسيستم (سليم)
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return appointment.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public async Task<IEnumerable<BookingReadDTO>?> GetAppointmentsByUserID(string userId)
        {
            // التعديل هنا: نفلتر القائمة بناءً على تاريخ الزيارة الطبي الفعلي (Date) وليس تاريخ كتابة السجل (AppoimentDate)
            // لكي تظهر المواعيد المستقبلية والماضية للمريض بشكل يعتمد تماماً على يوم الزيارة
            var todayOnly = DateOnly.FromDateTime(DateTime.Today);
            var limitDate = todayOnly.AddDays(-90); // آخر 90 يوماً من الزيارات الطبية

            var appointmentsList = await _context.Appointments
                .Include(a => a.doctor)
                .Include(a => a.patient)
                .Where(a => a.UserId == userId && a.Date >= limitDate) // التعديل هنا للفلترة بـ Date المختار
                .OrderByDescending(a => a.Date) // الترتيب بناءً على تاريخ الزيارة
                .ThenByDescending(a => a.Time)
                .AsNoTracking()
                .ToListAsync();

            if (!appointmentsList.Any()) return new List<BookingReadDTO>();

            var now = DateTime.Now;
            bool needToSave = false;

            // 2. تطبيق الـ Lazy Update بناءً على تاريخ ووقت الزيارة الطبي الفعلي المدمجين
            foreach (var app in appointmentsList)
            {
                if (app.Status == enAppoimentStatus.confirmed)
                {
                    DateTime fullDateTime = app.Date.ToDateTime(TimeOnly.FromTimeSpan(app.Time)); // التعديل هنا

                    if (fullDateTime < now)
                    {
                        app.Status = enAppoimentStatus.Completed;
                        needToSave = true;
                    }
                }
            }

            if (needToSave)
            {
                await _context.SaveChangesAsync();
            }

            // 4. الـ Mapping والترتيب النهائي للفرونت إند بقراءة دقيقة من الحقول الصحيحة
            var dtoResult = appointmentsList
                .Select(a => new BookingReadDTO
                {
                    AppointmentId = a.Id,
                    DoctorNameAr = $"{a.doctor.FirstNameAr} {a.doctor.LastNameAr}",
                    DoctorNameEn = $"{a.doctor.FirstName} {a.doctor.LastName}",
                    SpecsialityAr = a.doctor.SpecialtyAr,
                    SpecsialityEn = a.doctor.Specialty,
                    AppointmentStatus = a.Status == enAppoimentStatus.confirmed ? "Confirmed" :
                                        a.Status == enAppoimentStatus.Completed ? "Completed" : "Canceled",
                    AppointmentPrice = a.Price,
                    DoctorGenderEn = a.doctor.Gender,
                    DoctorGenderAr = a.doctor.GenderAr,
                    AppointmentDate = a.Date.ToDateTime(TimeOnly.MinValue), // نقوم بتحويل الـ Date الطبي لعرضه بالـ DTO
                    Time = a.Time,
                    PatientName = a.patient.FullName,
                    PatientIdNumber = a.patient.IdNumber
                })
                .OrderByDescending(a => a.AppointmentStatus == "Confirmed" || a.AppointmentStatus == "Completed")
                .ToList();

            return dtoResult;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return false;

            if (appointment.Status == enAppoimentStatus.CancelledByPatient)
            {
                throw new Exception("هذا الموعد ملغى بالفعل.");
            }

            // التعديل هنا: دمج التاريخ الطبي الفعلي (Date) مع الوقت (Time) لحساب مهلة الـ 12 ساعة بدقة متناهية
            DateTime appointmentFullDateTime = appointment.Date.ToDateTime(TimeOnly.FromTimeSpan(appointment.Time));

            TimeSpan timeUntilAppointment = appointmentFullDateTime - DateTime.Now;

            if (timeUntilAppointment.TotalHours < 12)
            {
                throw new Exception("لا يمكن إلغاء الموعد قبل أقل من 12 ساعة من موعده المحدد.");
            }

            if (!appointment.IsReview && !string.IsNullOrEmpty(appointment.StripePaymentIntentId))
            {
                try
                {
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = appointment.StripePaymentIntentId,
                    };
                    var refundService = new RefundService();
                    await refundService.CreateAsync(refundOptions);
                }
                catch (StripeException ex)
                {
                    throw new Exception($"فشلت عملية استرداد الأموال من Stripe: {ex.Message}");
                }
            }

            appointment.Status = enAppoimentStatus.CancelledByPatient;
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
    }
}