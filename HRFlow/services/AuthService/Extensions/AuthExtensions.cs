using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace AuthService.Extensions
{
    public static class AuthExtensions
    {

        public static IServiceCollection ConfigurePersistence(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AuthContext>(options =>
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
                }).AddEntityFrameworkStores<AuthContext>()
                .AddDefaultTokenProviders();
            return services;
        }
    }
}

