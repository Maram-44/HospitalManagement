using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.InterfacesServices;
using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using HospitalManagement.DataAccess.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            // إعداد المفتاح السري لمرة واحدة
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<string> CreatePaymentIntentAsync(long amountInCents)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents, // Stripe يحسب بالسنات (مثلاً 6000 تعني 60 دولار)
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
            };

            var service = new PaymentIntentService();
            PaymentIntent intent = await service.CreateAsync(options);

            // هذا هو السكرت الذي سنرسله لـ React
            return intent.ClientSecret;
        }
    }
}
