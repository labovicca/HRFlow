using MediatR;

namespace Payroll.Application.Queries.PayrollRuns;

public record DownloadPayslipQuery(Guid PayrollRunId) : IRequest<PayslipDownloadResult?>;

public class PayslipDownloadResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
