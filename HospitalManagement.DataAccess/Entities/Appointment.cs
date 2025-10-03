using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.DataAccess.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        [ForeignKey("doctor")]
        public int DoctorId { get; set; }   
        public Doctor doctor { get; set; }
        [ForeignKey("patient")]
        public int PatientId { get; set; }
        public Patient patient { get; set; }
    }
}
