using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.ModelView
{
    public class DoctorVM
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Specialty { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Phone { get; set; }
        public int DepartmentId { get; set; }
    }
}
