using AutoMapper;
using MediatR;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Queries.PayrollRuns;

public class GetPayrollSummaryQueryHandler : IRequestHandler<GetPayrollSummaryQuery, PayrollSummaryDto>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IMapper _mapper;

    public GetPayrollSummaryQueryHandler(IPayrollRunRepository payrollRunRepository, IMapper mapper)
    {
        _payrollRunRepository = payrollRunRepository;
        _mapper = mapper;
    }

    public async Task<PayrollSummaryDto> Handle(GetPayrollSummaryQuery request, CancellationToken cancellationToken)
    {
        var payrollRuns = (await _payrollRunRepository.GetByPeriodAsync(request.Year, request.Month, cancellationToken))
            .ToList();

        var summary = new PayrollSummaryDto
        {
            Year = request.Year,
            Month = request.Month,
            TotalPayrollRuns = payrollRuns.Count,
            DraftCount = payrollRuns.Count(pr => pr.Status == PayrollStatus.Draft),
            CalculatedCount = payrollRuns.Count(pr => pr.Status == PayrollStatus.Calculated),
            ApprovedCount = payrollRuns.Count(pr => pr.Status == PayrollStatus.Approved),
            PaidCount = payrollRuns.Count(pr => pr.Status == PayrollStatus.Paid),
            CancelledCount = payrollRuns.Count(pr => pr.Status == PayrollStatus.Cancelled),
            PayrollRuns = _mapper.Map<List<PayrollRunListDto>>(payrollRuns)
        };

        // Calculate financial summary only from non-draft, non-cancelled runs
        var processedRuns = payrollRuns
            .Where(pr => pr.Status != PayrollStatus.Draft && pr.Status != PayrollStatus.Cancelled)
            .ToList();

        if (processedRuns.Any())
        {
            summary.TotalGrossSalary = processedRuns.Sum(pr => pr.GrossSalary);
            summary.TotalNetSalary = processedRuns.Sum(pr => pr.NetSalary);
            summary.TotalDeductions = processedRuns.Sum(pr => pr.TotalDeductions);
            summary.TotalAdditions = processedRuns.Sum(pr => pr.TotalAdditions);
            summary.AverageGrossSalary = processedRuns.Average(pr => pr.GrossSalary);
            summary.AverageNetSalary = processedRuns.Average(pr => pr.NetSalary);
            summary.MinNetSalary = processedRuns.Min(pr => pr.NetSalary);
            summary.MaxNetSalary = processedRuns.Max(pr => pr.NetSalary);
        }

        return summary;
    }
}
