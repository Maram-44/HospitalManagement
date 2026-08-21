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
        public int Id { get; set; }
        // الاسم بالعربي
        public string FirstNameAr { get; set; }
        public string SecondNameAr { get; set; }
        public string ThirdNameAr { get; set; }
        public string LastNameAr { get; set; }

        // الاسم بالإنجليزي
        public string FirstNameEn { get; set; }
        public string SecoundNameEn { get; set; }
        public string ThirdNameEn { get; set; }
        public string LastNameEn { get; set; }

        public string SpecialtyAr { get; set; }
        public string SpecialtyEn { get; set; }
        public decimal Price { get; set; }
        public string GenderAr { get; set; }
        public string GenderEn { get; set; }
        public int DepartmentId { get; set; }
        public string? ImagePath { get; set; }
        public string BranchAr { get; set; }
        public string BranchEn { get; set; }


        public string? NextAvailableDayEn { get; set; }
        public string? NextAvailableDayAr { get; set; }
    }
}
