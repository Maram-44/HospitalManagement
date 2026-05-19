using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class PaymentIntentRequestDTO
    {
        public int DoctorId { get; set; }
        public int? PatientId { get; set; }
        public DateOnly AppointmentDate { get; set; }
    }
}
