using HospitalManagement.DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.DataAccess.Entities
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }
        public TimeSpan Time { get; set; }
        public DateOnly Date {  get; set; }
        public DateTime AppoimentDate { get; set; }= DateTime.Now;
        public bool IsReview { get; set; }
        [ForeignKey("doctor")]
        public int DoctorId { get; set; }   
        public Doctor doctor { get; set; }
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient patient { get; set; }
        public enAppoimentStatus Status { get; set; }
        public decimal Price { get; set; }
        public string StripePaymentIntentId { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
