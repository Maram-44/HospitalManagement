using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.DataAccess.Entities
{
    public class Branch
    {
        [Key]
        public int Id { get; set; }
        public string BranchName { get; set; } = null!;
        public ICollection<Doctor> Doctors { get; set; }

    }
}
