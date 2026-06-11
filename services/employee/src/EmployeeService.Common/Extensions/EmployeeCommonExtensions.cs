using EmployeeService.Common.Data;
using EmployeeService.Common.DTOs.Employee;
using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;
using EmployeeService.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EmployeeService.Common.Extensions;

public static class EmployeeCommonExtensions
{
    public static void AddEmployeeServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EmployeeDbContext>(options =>
            options.UseNpgsql(configuration.GetValue<string>("DatabaseSettings:ConnectionString")));
        
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        
        services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<Employee, EmployeeForPayrollDto>()
                .ForMember(dest => dest.EmploymentType,
                    opt => opt.MapFrom(src => src.EmploymentType.ToString()))
                .ForMember(dest => dest.EmploymentStatus,
                    opt => opt.MapFrom(src => src.EmploymentStatus.ToString()));
            cfg.CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.EmploymentType,
                    opt => opt.MapFrom(src => src.EmploymentType.ToString()))
                .ForMember(dest => dest.EmploymentStatus,
                    opt => opt.MapFrom(src => src.EmploymentStatus.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.EmploymentType,
                    opt => opt.MapFrom(src => 
                        Enum.Parse<EmploymentType>(src.EmploymentType.ToString())))
                .ForMember(dest => dest.EmploymentStatus,
                    opt => opt.MapFrom(src => 
                        Enum.Parse<EmploymentStatus>(src.EmploymentStatus.ToString())));
            
            cfg.CreateMap<Employee, CreateEmployeeDto>()
                .ReverseMap()
                .ForMember(dest => dest.EmploymentType,
                    opt => opt.MapFrom(src => src.EmploymentType))
                .ForMember(dest => dest.EmploymentStatus,
                    opt => opt.MapFrom(src => src.EmploymentStatus));
            
            cfg.CreateMap<Employee, UpdateEmployeeDto>()
                .ReverseMap()
                .ForMember(dest => dest.EmploymentType,
                    opt => opt.MapFrom(src => src.EmploymentType))
                .ForMember(dest => dest.EmploymentStatus,
                    opt => opt.MapFrom(src => src.EmploymentStatus));
        });
    }
}
