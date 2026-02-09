using Payroll.Domain.Entities;

namespace Payroll.Application.Interfaces;

/// <summary>
/// Interface za generisanje platnog listića (payslip) u PDF formatu.
/// </summary>
public interface IPayslipGenerator
{
    /// <summary>
    /// Generiše PDF platni listić za dati payroll run.
    /// </summary>
    /// <returns>Putanja do generisanog PDF fajla</returns>
    Task<string> GenerateAsync(PayrollRun payrollRun, PayrollConfiguration configuration, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generiše PDF platni listić i vraća ga kao byte array (za direktan download).
    /// </summary>
    Task<byte[]> GenerateBytesAsync(PayrollRun payrollRun, PayrollConfiguration configuration, CancellationToken cancellationToken = default);
}
