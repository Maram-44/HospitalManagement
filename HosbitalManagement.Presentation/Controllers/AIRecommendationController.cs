using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.Services.InterfacesServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HosbitalManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIRecommendationController : ControllerBase
    {
        private readonly IAIRecommendationService _aiRecommendationService;

        public AIRecommendationController(IAIRecommendationService aiRecommendationService)
        {
            _aiRecommendationService = aiRecommendationService;
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> GetRecommendation([FromBody] AIRecommendationRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(request.Symptoms))
            {
                return BadRequest("Symptoms field is required.");
            }

            try
            {
                var result = await _aiRecommendationService.GetRecommendationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // يمكنك استبدال هذا بـ Logger الخاص بك إذا كان متوفراً
                return StatusCode(500, $"An error occurred while processing the AI recommendation: {ex.Message}");
            }
        }
    }
}
