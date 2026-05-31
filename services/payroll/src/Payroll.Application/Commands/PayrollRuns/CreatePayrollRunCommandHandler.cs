using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.Application.Commands.PayrollRuns;

public class CreatePayrollRunCommandHandler : IRequestHandler<CreatePayrollRunCommand, PayrollRunDto>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IMapper _mapper;

    public CreatePayrollRunCommandHandler(
        IPayrollRunRepository payrollRunRepository,
        IMapper mapper)
    {
        _payrollRunRepository = payrollRunRepository;
        _mapper = mapper;
    }

    public async Task<PayrollRunDto> Handle(CreatePayrollRunCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        // Check if payroll run already exists for this employee and period
        var existing = await _payrollRunRepository.GetByEmployeeAndPeriodAsync(
            request.EmployeeId, request.Year, request.Month, cancellationToken);
        
        if (existing != null)
        {
            throw new InvalidOperationException(
                $"Payroll run already exists for employee {request.EmployeeId} for period {request.Year}-{request.Month:D2}");
        }

        var payrollRun = new PayrollRun
        {
            EmployeeId = request.EmployeeId,
            Year = request.Year,
            Month = request.Month,
            Status = PayrollStatus.Draft,
            GrossSalary = 0,
            NetSalary = 0,
            TotalDeductions = 0,
            TotalAdditions = 0,
            CreatedBy = command.CreatedBy
        };

        // Add additional components if provided
        if (request.AdditionalComponents != null && request.AdditionalComponents.Any())
        {
            foreach (var componentRequest in request.AdditionalComponents)
            {
                var component = new SalaryComponent
                {
                    Id = Guid.NewGuid(),
                    ComponentType = Enum.Parse<ComponentType>(componentRequest.ComponentType, true),
                    Name = componentRequest.Name,
                    Amount = componentRequest.Amount,
                    Percentage = componentRequest.Percentage,
                    IsTaxable = componentRequest.IsTaxable,
                    CreatedAt = DateTime.UtcNow
                };
                payrollRun.Components.Add(component);
            }
        }

        var created = await _payrollRunRepository.AddAsync(payrollRun, cancellationToken);
        
        return _mapper.Map<PayrollRunDto>(created);
    }
}
