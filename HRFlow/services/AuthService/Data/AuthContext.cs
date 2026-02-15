using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data
{
    public class AuthContext : IdentityDbContext<User>
    {

        public AuthContext(DbContextOptions options)
        {
            
        }
    }
}