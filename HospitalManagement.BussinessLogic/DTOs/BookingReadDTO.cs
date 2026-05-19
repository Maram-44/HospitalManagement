using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class BookingReadDTO
    {
        public int AppointmentId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorGender { get; set; }
        public string Specsiality { get; set; }
        public string AppointmentStatus { get; set; }
        public decimal AppointmentPrice { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan Time {  get; set; }
        public string  PatientName { get; set; }
        public string PatientIdNumber { get; set; }
    }
}
