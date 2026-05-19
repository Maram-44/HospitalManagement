using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.DataAccess.Entities
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        public string FullName {  get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public string IdType { get; set; }
        public string IdNumber { get; set; }
        public string Nationality { get; set; } = null!;
        public ICollection<Appointment> PatientsAppointments { get; set; }
    }
}
