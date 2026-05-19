using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class CreateCheckoutRequest
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; } 
        public DateOnly SelectedDate { get; set; }
        public TimeSpan SelectedTime { get; set; }
        public string? UserId { get; set; }
    }
}
