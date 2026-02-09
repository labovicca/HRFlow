using AutoMapper;
using Payroll.Application.DTOs;
using Payroll.Domain.Entities;

namespace Payroll.Application.Mappings;

public class PayrollMappingProfile : Profile
{
    public PayrollMappingProfile()
    {
        // PayrollRun mappings
        CreateMap<PayrollRun, PayrollRunDto>()
            .ForMember(dest => dest.Components, opt => opt.MapFrom(src => src.Components));
        
        CreateMap<PayrollRun, PayrollRunListDto>();
        
        // SalaryComponent mappings
        CreateMap<SalaryComponent, SalaryComponentDto>()
            .ForMember(dest => dest.ComponentType, opt => opt.MapFrom(src => src.ComponentType.ToString()));
        
        // Payslip mappings
        CreateMap<Payslip, PayslipDto>();
        
        // PayrollConfiguration mappings
        CreateMap<PayrollConfiguration, PayrollConfigurationDto>();
        
        CreateMap<CreatePayrollConfigurationRequest, PayrollConfiguration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        
        CreateMap<UpdatePayrollConfigurationRequest, PayrollConfiguration>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
    }
}
