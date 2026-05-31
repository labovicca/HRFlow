using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Payroll.Infrastructure.Services;

/// <summary>
/// Implementacija IPayslipGenerator korišćenjem QuestPDF biblioteke.
/// Generiše profesionalni platni listić u PDF formatu.
/// </summary>
public class PayslipPdfGenerator : IPayslipGenerator
{
    private readonly string _outputDirectory;

    public PayslipPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        _outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "payslips");
        Directory.CreateDirectory(_outputDirectory);
    }

    public Task<string> GenerateAsync(PayrollRun payrollRun, PayrollConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var fileName = $"payslip_{payrollRun.EmployeeId}_{payrollRun.Year}_{payrollRun.Month:D2}.pdf";
        var filePath = Path.Combine(_outputDirectory, fileName);

        var document = CreateDocument(payrollRun, configuration);
        document.GeneratePdf(filePath);

        return Task.FromResult(filePath);
    }

    public Task<byte[]> GenerateBytesAsync(PayrollRun payrollRun, PayrollConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var document = CreateDocument(payrollRun, configuration);
        var bytes = document.GeneratePdf();
        return Task.FromResult(bytes);
    }

    private Document CreateDocument(PayrollRun payrollRun, PayrollConfiguration configuration)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, payrollRun));
                page.Content().Element(c => ComposeContent(c, payrollRun, configuration));
                page.Footer().Element(ComposeFooter);
            });
        });
    }

    private void ComposeHeader(IContainer container, PayrollRun payrollRun)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("HRFlow - Payroll Service")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Platni listić / Payslip")
                        .FontSize(14).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(150).Column(col =>
                {
                    col.Item().AlignRight().Text($"Period: {payrollRun.Year}-{payrollRun.Month:D2}")
                        .FontSize(12).Bold();
                    col.Item().AlignRight().Text($"Status: {payrollRun.Status}")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
        });
    }

    private void ComposeContent(IContainer container, PayrollRun payrollRun, PayrollConfiguration configuration)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Employee Info Section
            column.Item().Element(c => ComposeEmployeeSection(c, payrollRun));
            column.Item().PaddingVertical(10);

            // Earnings Section
            column.Item().Element(c => ComposeEarningsSection(c, payrollRun, configuration));
            column.Item().PaddingVertical(10);

            // Deductions Section
            column.Item().Element(c => ComposeDeductionsSection(c, payrollRun, configuration));
            column.Item().PaddingVertical(10);

            // Summary Section
            column.Item().Element(c => ComposeSummarySection(c, payrollRun));
        });
    }

    private void ComposeEmployeeSection(IContainer container, PayrollRun payrollRun)
    {
        container.Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("PODACI O ZAPOSLENOM").FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Employee ID: {payrollRun.EmployeeId}").FontSize(9);
                row.RelativeItem().Text($"Payroll Run ID: {payrollRun.Id}").FontSize(9);
            });
            column.Item().Row(row =>
            {
                row.RelativeItem().Text($"Kreiran: {payrollRun.CreatedAt:dd.MM.yyyy}").FontSize(9);
                row.RelativeItem().Text($"Kreirao: {payrollRun.CreatedBy}").FontSize(9);
            });
        });
    }

    private void ComposeEarningsSection(IContainer container, PayrollRun payrollRun, PayrollConfiguration configuration)
    {
        container.Column(column =>
        {
            column.Item().Text("PRIMANJA").FontSize(11).Bold().FontColor(Colors.Green.Darken2);
            column.Item().PaddingTop(5);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Green.Lighten4).Padding(5).Text("Stavka").Bold();
                    header.Cell().Background(Colors.Green.Lighten4).Padding(5).AlignRight().Text("Procenat").Bold();
                    header.Cell().Background(Colors.Green.Lighten4).Padding(5).AlignRight().Text("Iznos (RSD)").Bold();
                });

                // Base salary
                table.Cell().Padding(5).Text("Osnovna plata");
                table.Cell().Padding(5).AlignRight().Text("-");
                table.Cell().Padding(5).AlignRight().Text($"{configuration.BaseSalary:N2}");

                // Additions from components
                var additions = payrollRun.Components
                    .Where(c => c.ComponentType == ComponentType.Addition)
                    .ToList();

                foreach (var addition in additions)
                {
                    table.Cell().Padding(5).Text(addition.Name);
                    table.Cell().Padding(5).AlignRight().Text(
                        addition.Percentage.HasValue ? $"{addition.Percentage.Value * 100:F2}%" : "-");
                    table.Cell().Padding(5).AlignRight().Text($"{addition.Amount:N2}");
                }

                // Total earnings
                table.Cell().Background(Colors.Green.Lighten5).Padding(5).Text("UKUPNA PRIMANJA").Bold();
                table.Cell().Background(Colors.Green.Lighten5).Padding(5).Text("");
                table.Cell().Background(Colors.Green.Lighten5).Padding(5).AlignRight()
                    .Text($"{payrollRun.GrossSalary:N2}").Bold();
            });
        });
    }

    private void ComposeDeductionsSection(IContainer container, PayrollRun payrollRun, PayrollConfiguration configuration)
    {
        container.Column(column =>
        {
            column.Item().Text("ODBICI").FontSize(11).Bold().FontColor(Colors.Red.Darken2);
            column.Item().PaddingTop(5);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Red.Lighten4).Padding(5).Text("Stavka").Bold();
                    header.Cell().Background(Colors.Red.Lighten4).Padding(5).AlignRight().Text("Stopa").Bold();
                    header.Cell().Background(Colors.Red.Lighten4).Padding(5).AlignRight().Text("Iznos (RSD)").Bold();
                });

                // Tax
                decimal taxAmount = payrollRun.GrossSalary * configuration.TaxRate;
                table.Cell().Padding(5).Text("Porez na dohodak");
                table.Cell().Padding(5).AlignRight().Text($"{configuration.TaxRate * 100:F2}%");
                table.Cell().Padding(5).AlignRight().Text($"{taxAmount:N2}");

                // Pension
                decimal pensionAmount = payrollRun.GrossSalary * configuration.PensionContributionRate;
                table.Cell().Padding(5).Text("Doprinos za PIO");
                table.Cell().Padding(5).AlignRight().Text($"{configuration.PensionContributionRate * 100:F2}%");
                table.Cell().Padding(5).AlignRight().Text($"{pensionAmount:N2}");

                // Health
                decimal healthAmount = payrollRun.GrossSalary * configuration.HealthInsuranceRate;
                table.Cell().Padding(5).Text("Doprinos za zdravstveno");
                table.Cell().Padding(5).AlignRight().Text($"{configuration.HealthInsuranceRate * 100:F2}%");
                table.Cell().Padding(5).AlignRight().Text($"{healthAmount:N2}");

                // Unemployment
                decimal unemploymentAmount = payrollRun.GrossSalary * configuration.UnemploymentInsuranceRate;
                table.Cell().Padding(5).Text("Doprinos za nezaposlenost");
                table.Cell().Padding(5).AlignRight().Text($"{configuration.UnemploymentInsuranceRate * 100:F2}%");
                table.Cell().Padding(5).AlignRight().Text($"{unemploymentAmount:N2}");

                // Deduction components
                var deductions = payrollRun.Components
                    .Where(c => c.ComponentType == ComponentType.Deduction)
                    .ToList();

                foreach (var deduction in deductions)
                {
                    table.Cell().Padding(5).Text(deduction.Name);
                    table.Cell().Padding(5).AlignRight().Text(
                        deduction.Percentage.HasValue ? $"{deduction.Percentage.Value * 100:F2}%" : "-");
                    table.Cell().Padding(5).AlignRight().Text($"{deduction.Amount:N2}");
                }

                // Total deductions
                table.Cell().Background(Colors.Red.Lighten5).Padding(5).Text("UKUPNI ODBICI").Bold();
                table.Cell().Background(Colors.Red.Lighten5).Padding(5).Text("");
                table.Cell().Background(Colors.Red.Lighten5).Padding(5).AlignRight()
                    .Text($"{payrollRun.TotalDeductions:N2}").Bold();
            });
        });
    }

    private void ComposeSummarySection(IContainer container, PayrollRun payrollRun)
    {
        container.Background(Colors.Blue.Lighten5).Padding(15).Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("BRUTO PLATA:").FontSize(12).Bold();
                row.ConstantItem(150).AlignRight().Text($"{payrollRun.GrossSalary:N2} RSD").FontSize(12).Bold();
            });

            column.Item().PaddingVertical(3).Row(row =>
            {
                row.RelativeItem().Text("Ukupni odbici:").FontSize(11);
                row.ConstantItem(150).AlignRight().Text($"- {payrollRun.TotalDeductions:N2} RSD")
                    .FontSize(11).FontColor(Colors.Red.Darken2);
            });

            column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("NETO PLATA:").FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                row.ConstantItem(150).AlignRight().Text($"{payrollRun.NetSalary:N2} RSD")
                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Generisano: {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
                row.RelativeItem().AlignRight().Text("HRFlow Payroll Service © 2024")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
