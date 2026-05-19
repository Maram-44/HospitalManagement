using AutoMapper;
using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using HospitalManagement.DataAccess.Enums;
using HospitalManagement.DataAccess.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    internal class AppoimentServices : IAppoimentServices
    {
        private readonly AppDbContext _context;
        private readonly IpatientServices _patientServices;
        private readonly IDoctorServices _doctorServices; // إضافة الخدمة هنا

        public AppoimentServices(AppDbContext context, IpatientServices patientServices, IDoctorServices doctorServices)
        {
            _context = context;
            _patientServices = patientServices;
            _doctorServices = doctorServices; // ربطها
        }

        public async Task<decimal> GetExpectedPrice(int doctorId, int? patientId, DateTime appointmentDate)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null) return 0;

            if (patientId.HasValue)
            {
                var now = DateTime.Now;

                // 1. جلب آخر حجز "كشف" للمريض مع هذا الطبيب بشرط أن يكون:
                // إما مؤكداً (Confirmed) أو مكتملاً (Completed) ولم يتم إلغاؤه.
                var lastAppointment = await _context.Appointments
                    .Where(a => a.PatientId == patientId &&
                                a.DoctorId == doctorId &&
                                a.IsReview == false &&
                                (a.Status == enAppoimentStatus.confirmed || a.Status == enAppoimentStatus.Completed))
                    .OrderByDescending(a => a.AppoimentDate)
                    .ThenByDescending(a => a.Time)
                    .FirstOrDefaultAsync();

                if (lastAppointment != null)
                {
                    // 2. دمج التاريخ والوقت للموعد القديم لمعرفة لحظة بدئه الفعالية
                    TimeSpan appointmentTime = lastAppointment.Time;
                    DateTime fullAppointmentDateTime = lastAppointment.AppoimentDate.Date + appointmentTime;

                    // 3. حساب الفارق الزمني بين وقت الموعد القديم واللحظة الحالية
                    var timePassed = now - fullAppointmentDateTime;

                    // 💡 لوجيك التحديث التلقائي (في الخلفية عند الطلب):
                    // إذا كان الموعد في قاعدة البيانات ما زال 'Confirmed' ولكن وقته مر وانتهى (timePassed > 0)
                    // نقوم بتحديث حالته فوراً في قاعدة البيانات إلى 'Completed' لكي يتنظف السيستم تلقائياً
                    if (lastAppointment.Status == enAppoimentStatus.confirmed && timePassed.TotalSeconds > 0)
                    {
                        lastAppointment.Status = enAppoimentStatus.Completed;
                        await _context.SaveChangesAsync(); // حفظ الحالة الجديدة صامتاً
                    }

                    // 4. الشرط الطبي: يجب أن يكون الموعد قد بدأ فعلاً في الماضي، ولم يمر عليه أكثر من 7 أيام
                    if (timePassed.TotalDays >= 0 && timePassed.TotalDays <= 7)
                    {
                        return 0; // يستحق مراجعة مجانية!
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

                // 3. استدعاء الفنكشن الموحدة لحساب السعر والتحقق من المراجعة
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
                if (!DateTime.TryParseExact(request.Time, "hh:mm tt", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedTime))
                    return null;

                var appointment = new Appointment
                {
                    PatientId = patient.Id,
                    DoctorId = request.DoctorId,
                    Date = request.Date,
                    Time = parsedTime.TimeOfDay,
                    Price = finalPrice,
                    IsReview = isReview,
                    StripePaymentIntentId = isReview ? "FREE_REVIEW" : request.StripePaymentIntentId,
                    Status = enAppoimentStatus.confirmed,
                    UserId = userId
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
            // 1. جلب المواعيد الخام من قاعدة البيانات كـ List أولاً إلى الذاكرة
            var appointmentsList = await _context.Appointments
                .Include(a => a.doctor)
                .Include(a => a.patient)
                .Where(a => a.UserId == userId && a.AppoimentDate >= DateTime.Today.AddDays(-90))
                .OrderByDescending(a => a.AppoimentDate)
                .ThenByDescending(a => a.Time)
                .ToListAsync();

            if (!appointmentsList.Any()) return new List<BookingReadDTO>();

            var now = DateTime.Now;
            bool needToSave = false;

            // 2. تطبيق الـ Lazy Update في الذاكرة (Memory) لكل موعد انتهى وقته
            foreach (var app in appointmentsList)
            {
                if (app.Status == enAppoimentStatus.confirmed)
                {
                    DateTime fullDateTime = app.AppoimentDate.Date + app.Time;

                    // إذا مر وقت الموعد في الماضي، حوله فوراً إلى Completed
                    if (fullDateTime < now)
                    {
                        app.Status = enAppoimentStatus.Completed; // تحديث في الذاكرة
                        needToSave = true; // نرفع راية تخبرنا بضرورة الحفظ في قاعدة البيانات
                    }
                }
            }

            // 3. إذا تم تحديث أي موعد، نحفظ التعديلات في قاعدة البيانات صامتاً في الخلفية
            if (needToSave)
            {
                await _context.SaveChangesAsync();
            }

            // 4. الآن نقوم بعمل الـ Mapping إلى الـ DTO والترتيب النهائي الفخم للمستخدم
            var dtoResult = appointmentsList
                .Select(a => new BookingReadDTO
                {
                    AppointmentId = a.Id,
                    DoctorName = $"Dr. {a.doctor.FirstName} {a.doctor.LastName}",
                    Specsiality = a.doctor.Specialty,

                    // ستقرأ الـ Completed التي حدثناها بالأعلى فوراً
                    AppointmentStatus = a.Status == enAppoimentStatus.confirmed ? "Confirmed" :
                                        a.Status == enAppoimentStatus.Completed ? "Completed" : "Canceled",

                    AppointmentPrice = a.Price,
                    DoctorGender = a.doctor.gender,
                    AppointmentDate = a.AppoimentDate,
                    Time = a.Time,
                    PatientName = a.patient.FullName,
                    PatientIdNumber = a.patient.IdNumber
                })
                // ترتيب يضع الـ Confirmed والـ Completed أولاً، ثم المكنسل في الأسفل
                .OrderByDescending(a => a.AppointmentStatus == "Confirmed" || a.AppointmentStatus == "Completed")
                .ToList();

            return dtoResult;
        }

        public async Task<bool> CancelAppointmentAsync(int appointmentId)
        {
            // 1. جلب الموعد مباشرة من الكونتكست مع التأكد من وجوده
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null) return false;

            // 2. التحقق من أن الموعد ليس مكنسل أصلاً
            if (appointment.Status == enAppoimentStatus.CancelledByPatient)
            {
                throw new Exception("هذا الموعد ملغى بالفعل.");
            }

            // 3. التحقق من شرط الـ 12 ساعة (باستخدام AppoimentDate الموجود في الـ Entity)
            // 1. دمج التاريخ والوقت من قاعدة البيانات في متغير واحد
            // افترضنا أن appointment.AppointmentDate هو التاريخ و appointment.Time هو TimeSpan
            DateTime appointmentFullDateTime = appointment.AppoimentDate.Date.Add(appointment.Time);

            // 2. حساب الفرق بين وقت الموعد والوقت الحالي
            TimeSpan timeUntilAppointment = appointmentFullDateTime - DateTime.Now;

            // 3. التحقق من شرط الـ 12 ساعة
            if (timeUntilAppointment.TotalHours < 12)
            {
                throw new Exception("لا يمكن إلغاء الموعد قبل أقل من 12 ساعة من موعده المحدد.");
            }

            // 4. منطق الاسترداد المالي عبر Stripe
            // الشرط: ليس مراجعة (IsReview == false) ويوجد معرف دفع
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
                    // تسجيل الخطأ أو رميه لإعلام الواجهة بفشل الاسترداد
                    throw new Exception($"فشلت عملية استرداد الأموال من Stripe: {ex.Message}");
                }
            }

            // 5. تحديث الحالة في قاعدة البيانات
            appointment.Status = enAppoimentStatus.CancelledByPatient;

            // 6. حفظ التغييرات مباشرة عبر الكونتكست
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }
    }
}
