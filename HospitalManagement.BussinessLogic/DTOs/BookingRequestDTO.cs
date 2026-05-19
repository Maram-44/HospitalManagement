using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class BookingRequestDTO
    {
        // بيانات الحجز
        public int DoctorId { get; set; }
        public DateOnly Date { get; set; }
        public string Time { get; set; } = null!;
        public string StripePaymentIntentId { get; set; } = null!;

        // بيانات المريض (كائن متداخل)
        public PatientDTO PatientInfo { get; set; }
    }
}
