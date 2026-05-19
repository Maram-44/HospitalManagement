using HospitalManagement.BussinessLogic.DTOs;
using HospitalManagement.BussinessLogic.ModelView;
using HospitalManagement.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.BussinessLogic.InterfacesServices
{
    public interface IAppoimentServices
    {
        Task<decimal> GetExpectedPrice(int doctorId, int? patientId, DateTime appointmentDate);
        Task<int?> ProcessBookingAfterPayment(BookingRequestDTO request,string userId);
        Task<IEnumerable<BookingReadDTO?>> GetAppointmentsByUserID(string userId);
        Task<bool> CancelAppointmentAsync(int appointmentId);
    }
}
