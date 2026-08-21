using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.Mappers;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.BussinessLogic.Services.InterfacesServices;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    public class AIRecommendationService : IAIRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public AIRecommendationService(
            AppDbContext context,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<AIRecommendationResponseDto> GetRecommendationAsync(AIRecommendationRequestDto request)
        {
            // 1. جلب كافة الأقسام لإرسالها للذكاء الاصطناعي
            var departments = await _context.Departments
                .AsNoTracking()
                .ToListAsync();

            // الخطوة الأولى: جعل الذكاء الاصطناعي يختار القسم والخطورة بناءً على الأعراض
            var departmentPrompt = BuildDepartmentPrompt(request, departments);
            var departmentJson = await AskGroq(departmentPrompt);
            var departmentResult = JsonSerializer.Deserialize<GeminiDepartmentSelectionDto>(departmentJson, _jsonOptions);

            if (departmentResult == null || departmentResult.DepartmentId <= 0)
            {
                throw new Exception("AI failed to determine a valid department.");
            }

            // 2. جلب الأطباء في القسم الذي اختاره الذكاء الاصطناعي
            var today = DateOnly.FromDateTime(DateTime.Today);

            var doctorsInDepartment = await _context.Doctors
                .Include(x => x.department)
                .Include(x => x.branch)
                .Where(x => x.DepartmentId == departmentResult.DepartmentId)
                .AsNoTracking()
                .ToListAsync();

            var doctorIds = doctorsInDepartment.Select(d => d.Id).ToList();

            // جلب إجازات أطباء هذا القسم فقط
            var allLeaves = await _context.DoctorLeaves
                .Where(x => doctorIds.Contains(x.DoctorId) && x.EndDate >= today)
                .AsNoTracking()
                .ToListAsync();

            // 3. تصفية الأطباء المتاحين وتحويلهم مباشرة إلى DTOs مع حساب اليوم المتاح عبر المابير
            var availableDoctorsDto = new List<DoctorDTO>();

            foreach (var d in doctorsInDepartment)
            {
                var currentDoctorLeaves = allLeaves.Where(l => l.DoctorId == d.Id).ToList();

                // التحقق ما إذا كان الطبيب في إجازة اليوم
                bool isOnLeaveToday = currentDoctorLeaves.Any(l => today >= l.StartDate && today <= l.EndDate);

                if (!isOnLeaveToday)
                {
                    // استدعاء المابير الثابت وتمرير الإجازات ليقوم بحساب الأيام المتاحة وتعبئة الـ DTO
                    availableDoctorsDto.Add(d.ToDto(currentDoctorLeaves));
                }
            }

            // إذا لم يتوفر أطباء في هذا القسم اليوم
            if (!availableDoctorsDto.Any())
            {
                var chosenDep = departments.FirstOrDefault(d => d.Id == departmentResult.DepartmentId);
                return new AIRecommendationResponseDto
                {
                    DepartmentId = departmentResult.DepartmentId,
                    DepartmentName = chosenDep?.Name ?? "Unknown",
                    Priority = departmentResult.Priority,
                    Reason = "No doctors available in the recommended department at the moment.",
                    Emergency = departmentResult.Emergency
                };
            }

            // الخطوة الثانية: إرسال الـ DTOs الجاهزة والمحسوبة ليختار الذكاء الاصطناعي الأنسب بينهم
            var doctorPrompt = BuildDoctorPrompt(request, availableDoctorsDto);
            var doctorJson = await AskGroq(doctorPrompt);
            var doctorResult = JsonSerializer.Deserialize<GeminiDoctorSelectionDto>(doctorJson, _jsonOptions);

            // جلب الطبيب الذي تم اختياره من القائمة المحسوبة مسبقاً
            var selectedDoctorDto = availableDoctorsDto.FirstOrDefault(x => x.Id == doctorResult.DoctorId) ?? availableDoctorsDto.First();

            return new AIRecommendationResponseDto
            {
                DepartmentId = departmentResult.DepartmentId,
                DepartmentName = selectedDoctorDto.BranchEn, // أو اسم القسم المناسب
                DoctorId = selectedDoctorDto.Id,
                DoctorName = selectedDoctorDto.FirstNameEn + " " + selectedDoctorDto.LastNameEn,
                Priority = departmentResult.Priority,
                Reason = doctorResult.Reason,
                Emergency = departmentResult.Emergency
            };
        }

        private string BuildDepartmentPrompt(AIRecommendationRequestDto request, List<Department> departments)
        {
            var sb = new StringBuilder();
            sb.AppendLine("""
You are an AI Medical Triage Assistant. Your task is Phase 1: Determine the correct department based on patient symptoms.

Your responsibilities:
1. Recommend ONLY ONE department from the provided list.
2. Determine urgency level (Low, Medium, High).
3. Return ONLY valid JSON.
""");

            AppendPatientInfo(sb, request);

            sb.AppendLine("\n===== Available Departments =====");
            foreach (var dep in departments)
            {
                sb.AppendLine($"{dep.Id} - {dep.Name}");
            }

            sb.AppendLine("""

Return ONLY this JSON format:
{
  "departmentId": 0,
  "priority": "Low",
  "emergency": false
}
""");
            return sb.ToString();
        }

        // تم تعديل نوع القائمة هنا لتستقبل DoctorDTO بدلاً من Entity لكي يقرأ الحقول المحسوبة بدقة
        private string BuildDoctorPrompt(AIRecommendationRequestDto request, List<DoctorDTO> doctors)
        {
            var sb = new StringBuilder();
            sb.AppendLine("""
You are an AI Medical Triage Assistant. Your task is Phase 2: Select the best doctor from the provided list within the already chosen department.

Your responsibilities:
1. Recommend ONLY ONE doctor from the provided list who best matches the patient's case.
2. Briefly explain your recommendation in the reason field.
3. Return ONLY valid JSON.
""");

            AppendPatientInfo(sb, request);

            sb.AppendLine("\n===== Available Doctors in Chosen Department =====");
            foreach (var doctor in doctors)
            {
                sb.AppendLine($$"""
DoctorId: {{doctor.Id}}
Name: {{doctor.FirstNameEn}} {{doctor.LastNameEn}}
Specialty: {{doctor.SpecialtyEn}}
Gender: {{doctor.GenderEn}}
Branch: {{doctor.BranchEn}}
ConsultationPrice: {{doctor.Price}}
NextAvailableDay: {{doctor.NextAvailableDayEn}}

""");
            }

            sb.AppendLine("""
Return ONLY this JSON format:
{
  "doctorId": 0,
  "reason": ""
}
""");
            return sb.ToString();
        }

        private void AppendPatientInfo(StringBuilder sb, AIRecommendationRequestDto request)
        {
            sb.AppendLine("\n===== Patient Information =====");
            sb.AppendLine($"Age: {request.Age}");
            sb.AppendLine($"Gender: {request.Gender}");

            if (request.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase) && request.IsPregnant.HasValue)
            {
                sb.AppendLine($"Pregnant: {request.IsPregnant}");
            }

            if (request.ChronicDiseases != null && request.ChronicDiseases.Any())
            {
                sb.AppendLine("Chronic Diseases:");
                foreach (var disease in request.ChronicDiseases)
                    sb.AppendLine($"- {disease}");
            }
            sb.AppendLine($"Symptoms: {request.Symptoms}");
        }

        private async Task<string> AskGroq(string prompt)
        {
            var apiKey = _configuration["Groq:ApiKey"];
            var model = _configuration["Groq:Model"];

            // رابط الـ API الموحد لـ Groq (متوافق مع صيغة OpenAI)
            var url = "https://api.groq.com/openai/v1/chat/completions";

            // تجهيز الـ Payload وتحديد صيغة الـ JSON الإلزامية
            var body = new
            {
                model = model,
                messages = new[]
                {
            new { role = "user", content = prompt }
        },
                response_format = new { type = "json_object" }, // لضمان رجوع JSON نظيف
                temperature = 0.1 // درجة حرارة منخفضة لضمان دقة الاختيار الطبي
            };

            var json = JsonSerializer.Serialize(body);

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            // تمرير مفتاح Groq في الـ Header كـ Bearer Token
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API Error (Status: {response.StatusCode}): {errorContent}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            // قراءة الـ JSON الراجع من Groq واستخراج النص
            using JsonDocument doc = JsonDocument.Parse(responseBody);

            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text!.Trim();
        }
    }

    // الـ DTOs الخاصة باستقبال البيانات من Gemini لكل مرحلة
    public class GeminiDepartmentSelectionDto
    {
        public int DepartmentId { get; set; }
        public string Priority { get; set; }
        public bool Emergency { get; set; }
    }

    public class GeminiDoctorSelectionDto
    {
        public int DoctorId { get; set; }
        public string Reason { get; set; }
    }
}