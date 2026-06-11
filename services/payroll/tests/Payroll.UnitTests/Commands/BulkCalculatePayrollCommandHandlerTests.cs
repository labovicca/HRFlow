using FluentAssertions;
using Moq;
using Payroll.Application.Commands.PayrollRuns;
using Payroll.Application.DTOs;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.UnitTests.Commands;

public class BulkCalculatePayrollCommandHandlerTests
{
    private readonly Mock<IPayrollRunRepository> _runRepoMock;
    private readonly Mock<IPayrollConfigurationRepository> _configRepoMock;
    private readonly Mock<IPayrollCalculator> _calculatorMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly BulkCalculatePayrollCommandHandler _handler;

    public BulkCalculatePayrollCommandHandlerTests()
    {
        _runRepoMock = new Mock<IPayrollRunRepository>();
        _configRepoMock = new Mock<IPayrollConfigurationRepository>();
        _calculatorMock = new Mock<IPayrollCalculator>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _handler = new BulkCalculatePayrollCommandHandler(
            _runRepoMock.Object,
            _configRepoMock.Object,
            _calculatorMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_MultipleDraftRuns_ShouldCalculateAll()
    {
        // Arrange
        var emp1 = Guid.NewGuid();
        var emp2 = Guid.NewGuid();
        var run1 = new PayrollRun { Id = Guid.NewGuid(), EmployeeId = emp1, Year = 2026, Month = 1, Status = PayrollStatus.Draft, Components = new List<SalaryComponent>() };
        var run2 = new PayrollRun { Id = Guid.NewGuid(), EmployeeId = emp2, Year = 2026, Month = 1, Status = PayrollStatus.Draft, Components = new List<SalaryComponent>() };
        var nonDraftRun = new PayrollRun { Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1, Status = PayrollStatus.Approved, Components = new List<SalaryComponent>() };

        _runRepoMock
            .Setup(r => r.GetByPeriodAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PayrollRun> { run1, run2, nonDraftRun });

        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(run1.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(run1);
        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(run2.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(run2);

        var config = new PayrollConfiguration { BaseSalary = 100000m, TaxRate = 0.10m, PensionContributionRate = 0.14m, HealthInsuranceRate = 0.0515m, UnemploymentInsuranceRate = 0.0075m };
        _configRepoMock
            .Setup(r => r.GetEffectiveConfigurationAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        var calcResult = new PayrollCalculationResult { GrossSalary = 100000m, NetSalary = 80100m, TotalDeductions = 19900m, TotalAdditions = 0m };
        _calculatorMock
            .Setup(c => c.CalculateAsync(It.IsAny<PayrollRun>(), config, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calcResult);

        var command = new BulkCalculatePayrollCommand(2026, 1, "system");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalProcessed.Should().Be(2); // Only Draft runs
        result.Successful.Should().Be(2);
        result.Failed.Should().Be(0);
        result.Errors.Should().BeEmpty();

        _runRepoMock.Verify(r => r.UpdateAsync(It.IsAny<PayrollRun>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<PayrollCalculatedEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_NoConfigurationForEmployee_ShouldReportError()
    {
        // Arrange
        var run = new PayrollRun { Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1, Status = PayrollStatus.Draft, Components = new List<SalaryComponent>() };

        _runRepoMock
            .Setup(r => r.GetByPeriodAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PayrollRun> { run });

        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(run.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(run);

        _configRepoMock
            .Setup(r => r.GetEffectiveConfigurationAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollConfiguration?)null);

        var command = new BulkCalculatePayrollCommand(2026, 1, "system");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalProcessed.Should().Be(1);
        result.Successful.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].EmployeeId.Should().Be(run.EmployeeId);
    }

    [Fact]
    public async Task Handle_NoDraftRuns_ShouldReturnEmptyResult()
    {
        // Arrange
        var approvedRun = new PayrollRun { Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1, Status = PayrollStatus.Approved, Components = new List<SalaryComponent>() };

        _runRepoMock
            .Setup(r => r.GetByPeriodAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PayrollRun> { approvedRun });

        var command = new BulkCalculatePayrollCommand(2026, 1, "system");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalProcessed.Should().Be(0);
        result.Successful.Should().Be(0);
        result.Failed.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CalculationException_ShouldCatchAndReportError()
    {
        // Arrange
        var run = new PayrollRun { Id = Guid.NewGuid(), EmployeeId = Guid.NewGuid(), Year = 2026, Month = 1, Status = PayrollStatus.Draft, Components = new List<SalaryComponent>() };

        _runRepoMock
            .Setup(r => r.GetByPeriodAsync(2026, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PayrollRun> { run });

        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(run.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(run);

        var config = new PayrollConfiguration { BaseSalary = 100000m };
        _configRepoMock
            .Setup(r => r.GetEffectiveConfigurationAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _calculatorMock
            .Setup(c => c.CalculateAsync(It.IsAny<PayrollRun>(), config, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Calculation error"));

        var command = new BulkCalculatePayrollCommand(2026, 1, "system");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalProcessed.Should().Be(1);
        result.Successful.Should().Be(0);
        result.Failed.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Contain("Calculation error");
    }
}
