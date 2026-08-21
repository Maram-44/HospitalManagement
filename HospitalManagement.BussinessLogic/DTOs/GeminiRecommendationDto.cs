using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class GeminiRecommendationDto
    {
        public int DepartmentId { get; set; }

        public int DoctorId { get; set; }

        public string Priority { get; set; }

        public string Reason { get; set; }

        public bool Emergency { get; set; }
    }
}
