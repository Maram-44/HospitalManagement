using HospitalManagement.BussinessLogic.Services;
using HospitalManagement.BussinessLogic.Services.InterfacesServices;
using HospitalManagement.BussinessLogic.Tools;
using HospitalManagement.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
        {
            //services.AddAutoMapper(typeof(Mapping));
            var conn = new ConfigurationBuilder().AddJsonFile("setting.json").Build();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conn.GetSection("conn").Value));
            services.AddScoped(typeof(IpatientServices),typeof(PatientServices));
            services.AddScoped(typeof(IAuthService), typeof(AuthService));
            services.AddScoped(typeof(IDoctorServices), typeof(DoctorServices));
            services.AddScoped(typeof(IDepartmentServices), typeof(DepartmentServices));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAppoimentServices, AppoimentServices>();
            services.AddScoped<IAIRecommendationService, AIRecommendationService>();

            return services;
        }
    }
}
