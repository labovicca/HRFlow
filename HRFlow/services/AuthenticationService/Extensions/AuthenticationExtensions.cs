using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthenticationService.Data;
using AuthenticationService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace AuthenticationService.Extensions
{
    public static class AuthenticationExtensions
    {

        public static IServiceCollection ConfigurePersistence(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AuthenticationContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("IdentityConnection"));
            });
            return services;
        }

        public static IServiceCollection ConfigurationAuthentication(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 8;
                    options.User.RequireUniqueEmail = true;
                }).AddEntityFrameworkStores<AuthenticationContext>()
                .AddDefaultTokenProviders();
            return services;
        }
    }
}

