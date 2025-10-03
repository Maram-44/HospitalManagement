using HospitalManagement.DataAccess.Data;
using HospitalManagement.DataAccess.Entities;
using HospitalManagement.DataAccess.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalManagement.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public User? FindByUserName(string userName)
        {
            return _context.Users.Include(u => u.Role).FirstOrDefault(u => u.Username == userName);
        }
    }
}
