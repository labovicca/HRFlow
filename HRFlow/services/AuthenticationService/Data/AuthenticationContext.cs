using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationService.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Data
{
    public class AuthenticationContext : IdentityDbContext<User>
    {

        public AuthenticationContext(DbContextOptions options)
        {
            
        }
    }
}