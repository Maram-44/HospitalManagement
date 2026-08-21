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
        public string DoctorNameAr { get; set; }
        public string DoctorNameEn { get; set; }
        public string DoctorGenderAr { get; set; }
        public string DoctorGenderEn { get; set; }
        public string SpecsialityAr { get; set; }
        public string SpecsialityEn { get; set; }
        public string AppointmentStatus { get; set; }
        public decimal AppointmentPrice { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan Time {  get; set; }
        public string  PatientName { get; set; }
        public string PatientIdNumber { get; set; }
    }
}
