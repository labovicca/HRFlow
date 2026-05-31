using MediatR;
using Payroll.Application.Interfaces;
using Payroll.Domain.Enums;

namespace Payroll.Application.Queries.PayrollRuns;

public class DownloadPayslipQueryHandler : IRequestHandler<DownloadPayslipQuery, PayslipDownloadResult?>
{
    private readonly IPayrollRunRepository _payrollRunRepository;
    private readonly IPayrollConfigurationRepository _configurationRepository;
    private readonly IPayslipGenerator _payslipGenerator;

    public DownloadPayslipQueryHandler(
        IPayrollRunRepository payrollRunRepository,
        IPayrollConfigurationRepository configurationRepository,
        IPayslipGenerator payslipGenerator)
    {
        _payrollRunRepository = payrollRunRepository;
        _configurationRepository = configurationRepository;
        _payslipGenerator = payslipGenerator;
    }

    public async Task<PayslipDownloadResult?> Handle(DownloadPayslipQuery request, CancellationToken cancellationToken)
    {
        var payrollRun = await _payrollRunRepository.GetByIdWithComponentsAsync(request.PayrollRunId, cancellationToken);

        if (payrollRun == null)
        {
            return null;
        }

        if (payrollRun.Status != PayrollStatus.Calculated && 
            payrollRun.Status != PayrollStatus.Approved && 
            payrollRun.Status != PayrollStatus.Paid)
        {
            return null;
        }

        var periodDate = new DateTime(payrollRun.Year, payrollRun.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var configuration = await _configurationRepository.GetEffectiveConfigurationAsync(
            payrollRun.EmployeeId, periodDate, cancellationToken);

        if (configuration == null)
        {
            return null;
        }

        var pdfBytes = await _payslipGenerator.GenerateBytesAsync(payrollRun, configuration, cancellationToken);

        return new PayslipDownloadResult
        {
            FileBytes = pdfBytes,
            FileName = $"payslip_{payrollRun.EmployeeId}_{payrollRun.Year}_{payrollRun.Month:D2}.pdf",
            ContentType = "application/pdf"
        };
    }
}
