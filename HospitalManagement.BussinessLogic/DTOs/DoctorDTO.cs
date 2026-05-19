using HospitalManagement.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.ModelView
{
    public class DoctorDTO
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }
        [EmailAddress]
        public decimal Price { get; set; }
        public string Gender { get; set; }
        public int DepartmentId { get; set; }
        public string Branch { get; set; }


        public string? NextAvailableDay { get; set; }
    }
}
