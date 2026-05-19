using HospitalManagement.BussinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.InterfacesServices
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentIntentAsync(long amountInCents);
    }
}
