using AutoMapper;
using FluentAssertions;
using Moq;
using Payroll.Application.Interfaces;
using Payroll.Application.Mappings;
using Payroll.Application.Queries.PayrollRuns;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.UnitTests.Queries;

public class GetPayrollSummaryQueryHandlerTests
{
    private readonly Mock<IPayrollRunRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly GetPayrollSummaryQueryHandler _handler;

    public GetPayrollSummaryQueryHandlerTests()
    {
        _repositoryMock = new Mock<IPayrollRunRepository>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PayrollMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new GetPayrollSummaryQueryHandler(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WithMultipleRuns_ShouldCalculateCorrectSummary()
    {
        // Arrange
        var runs = new List<PayrollRun>
        {
            new PayrollRun
            {
                Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1,
                Status = PayrollStatus.Paid,
                GrossSalary = 120000m, NetSalary = 96060m, TotalDeductions = 23940m, TotalAdditions = 0m,
                Components = new List<SalaryComponent>()
            },
            new PayrollRun
            {
                Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1,
                Status = PayrollStatus.Approved,
                GrossSalary = 85000m, NetSalary = 68042.50m, TotalDeductions = 16957.50m, TotalAdditions = 0m,
                Components = new List<SalaryComponent>()
            },
            new PayrollRun
            {
                Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1,
                Status = PayrollStatus.Draft, // Should NOT be included in financial summary
                GrossSalary = 0m, NetSalary = 0m, TotalDeductions = 0m, TotalAdditions = 0m,
                Components = new List<SalaryComponent>()
            }
        };

        _repositoryMock
            .Setup(r => r.GetByPeriodAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        var query = new GetPayrollSummaryQuery(2026, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Year.Should().Be(2026);
        result.Month.Should().Be(1);
        result.TotalPayrollRuns.Should().Be(3);
        result.PaidCount.Should().Be(1);
        result.ApprovedCount.Should().Be(1);
        result.DraftCount.Should().Be(1);
        
        // Financial summary should only include Paid and Approved
        result.TotalGrossSalary.Should().Be(120000m + 85000m);
        result.TotalNetSalary.Should().Be(96060m + 68042.50m);
        result.AverageGrossSalary.Should().Be((120000m + 85000m) / 2);
        result.MinNetSalary.Should().Be(68042.50m);
        result.MaxNetSalary.Should().Be(96060m);
    }

    [Fact]
    public async Task Handle_EmptyPeriod_ShouldReturnZeroSummary()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByPeriodAsync(2026, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PayrollRun>());

        var query = new GetPayrollSummaryQuery(2026, 3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalPayrollRuns.Should().Be(0);
        result.TotalGrossSalary.Should().Be(0);
        result.TotalNetSalary.Should().Be(0);
    }

    [Fact]
    public async Task Handle_OnlyDraftRuns_ShouldHaveZeroFinancials()
    {
        // Arrange
        var runs = new List<PayrollRun>
        {
            new PayrollRun
            {
                Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 2,
                Status = PayrollStatus.Draft,
                GrossSalary = 0m, NetSalary = 0m, TotalDeductions = 0m, TotalAdditions = 0m,
                Components = new List<SalaryComponent>()
            }
        };

        _repositoryMock
            .Setup(r => r.GetByPeriodAsync(2026, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        var query = new GetPayrollSummaryQuery(2026, 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalPayrollRuns.Should().Be(1);
        result.DraftCount.Should().Be(1);
        result.TotalGrossSalary.Should().Be(0);
        result.TotalNetSalary.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnPayrollRunListInResult()
    {
        // Arrange
        var runs = new List<PayrollRun>
        {
            new PayrollRun
            {
                Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1,
                Status = PayrollStatus.Paid,
                GrossSalary = 100000m, NetSalary = 80100m, TotalDeductions = 19900m, TotalAdditions = 0m,
                Components = new List<SalaryComponent>()
            }
        };

        _repositoryMock
            .Setup(r => r.GetByPeriodAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        var query = new GetPayrollSummaryQuery(2026, 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.PayrollRuns.Should().HaveCount(1);
        result.PayrollRuns[0].GrossSalary.Should().Be(100000m);
    }
}
