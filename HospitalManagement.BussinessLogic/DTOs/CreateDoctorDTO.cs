using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class CreateDoctorDto
    {
        public string FirstNameAr { get; set; }
        public string SecondNameAr { get; set; }
        public string ThirdNameAr { get; set; }
        public string LastNameAr { get; set; }

        public string FirstName { get; set; }
        public string SecoundName { get; set; }
        public string ThirdName { get; set; }
        public string LastName { get; set; }

        public string SpecialtyAr { get; set; }
        public string Specialty { get; set; }

        public string DescriptionAr { get; set; }
        public string Description { get; set; }

        public string GenderAr { get; set; }
        public string Gender { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public decimal Price { get; set; }

        public int DepartmentId { get; set; }
        public int? BranchId { get; set; }

        // هذا حقل الملف (الصورة) - في بوستمان اختر نوعه File
        public IFormFile? Image { get; set; }
    }
}
