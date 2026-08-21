using HospitalManagement.BussinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services.InterfacesServices
{
    public interface IAIRecommendationService
    {
        Task<AIRecommendationResponseDto> GetRecommendationAsync(AIRecommendationRequestDto request);
    }
}
