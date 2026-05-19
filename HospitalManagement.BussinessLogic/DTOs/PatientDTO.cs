using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class PatientDTO
    {
        public int? Id { get; set; }
        public string FullName { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Phone { get; set; }
        public string? Email { get; set; }
        public string IdType { get; set; }
        public string IdNumber { get; set; }
        public string? Nationality { get; set; }
    }
}
