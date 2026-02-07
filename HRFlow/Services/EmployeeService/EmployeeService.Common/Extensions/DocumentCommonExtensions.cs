using EmployeeService.Common.Data;
using EmployeeService.Common.DTOs.Document;
using EmployeeService.Common.Entities;
using EmployeeService.Common.Enums;
using EmployeeService.Common.Repositories;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;

namespace EmployeeService.Common.Extensions;

public static class DocumentCommonExtensions
{
    public static void AddDocumentServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeContext, EmployeeContext>();
        
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        
        services.AddAutoMapper(cfg =>
        {
            cfg.CreateMap<Document, DocumentDto>()
                .ForMember(dest => dest.Type, 
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
            
            cfg.CreateMap<DocumentDto, Document>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => 
                        Enum.Parse<DocumentType>(src.Type)))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => 
                        Enum.Parse<DocumentStatus>(src.Status)));
            
            cfg.CreateMap<Document, BaseIdentityDocumentDto>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => 
                        string.IsNullOrEmpty(src.Type) 
                            ? DocumentType.Other 
                            : Enum.Parse<DocumentType>(src.Type)))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => DocumentStatus.Uploaded));
            
            cfg.CreateMap<Document, CreateDocumentDto>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => 
                        string.IsNullOrEmpty(src.Type) 
                            ? DocumentType.Other 
                            : Enum.Parse<DocumentType>(src.Type)))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => DocumentStatus.Uploaded));
            
            cfg.CreateMap<Document, UpdateDocumentDto>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => 
                        string.IsNullOrEmpty(src.Type) 
                            ? DocumentType.Other 
                            : Enum.Parse<DocumentType>(src.Type)));
        });
    }
}