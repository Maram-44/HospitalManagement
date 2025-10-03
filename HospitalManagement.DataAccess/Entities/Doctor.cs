using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.DataAccess.Entities
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Phone { get; set; }
        [ForeignKey("department")]
        public int DepartmentId { get; set; }
        public Department department { get; set; }
        public ICollection<Appointment> DoctorAppointments { get; set; }

    }
}
