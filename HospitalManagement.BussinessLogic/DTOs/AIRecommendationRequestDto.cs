using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.DTOs
{
    public class AIRecommendationRequestDto
    {
        public int Age { get; set; }

        public string Gender { get; set; } = string.Empty;

        public bool? IsPregnant { get; set; }

        public List<string> ChronicDiseases { get; set; } = new();

        public string Symptoms { get; set; } = string.Empty;

        public string? City { get; set; }
    }
}
