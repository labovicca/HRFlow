using FluentAssertions;
using Moq;
using Payroll.Application.Commands.PayrollRuns;
using Payroll.Application.DTOs;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.UnitTests.Commands;

public class CalculatePayrollCommandHandlerTests
{
    private readonly Mock<IPayrollRunRepository> _runRepoMock;
    private readonly Mock<IPayrollConfigurationRepository> _configRepoMock;
    private readonly Mock<IPayrollCalculator> _calculatorMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly CalculatePayrollCommandHandler _handler;

    public CalculatePayrollCommandHandlerTests()
    {
        _runRepoMock = new Mock<IPayrollRunRepository>();
        _configRepoMock = new Mock<IPayrollConfigurationRepository>();
        _calculatorMock = new Mock<IPayrollCalculator>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _handler = new CalculatePayrollCommandHandler(
            _runRepoMock.Object,
            _configRepoMock.Object,
            _calculatorMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_ValidDraftRun_ShouldCalculateAndUpdateStatus()
    {
        // Arrange
        var runId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = employeeId,
            Year = 2024,
            Month = 3,
            Status = PayrollStatus.Draft,
            Components = new List<SalaryComponent>()
        };

        var config = new PayrollConfiguration
        {
            Id = Guid.NewGuid(),
            BaseSalary = 2000m,
            TaxRate = 0.10m,
            PensionContributionRate = 0.14m,
            HealthInsuranceRate = 0.0515m,
            UnemploymentInsuranceRate = 0.0075m
        };

        var calculationResult = new PayrollCalculationResult
        {
            PayrollRunId = runId,
            EmployeeId = employeeId,
            GrossSalary = 2000m,
            NetSalary = 1402m,
            TotalDeductions = 598m,
            TotalAdditions = 0m
        };

        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        _configRepoMock
            .Setup(r => r.GetEffectiveConfigurationAsync(employeeId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);

        _calculatorMock
            .Setup(c => c.CalculateAsync(payrollRun, config, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculationResult);

        var command = new CalculatePayrollCommand(runId, "system");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.GrossSalary.Should().Be(2000m);
        result.NetSalary.Should().Be(1402m);

        payrollRun.Status.Should().Be(PayrollStatus.Calculated);
        payrollRun.GrossSalary.Should().Be(2000m);
        payrollRun.NetSalary.Should().Be(1402m);
        payrollRun.UpdatedBy.Should().Be("system");

        _runRepoMock.Verify(r => r.UpdateAsync(payrollRun, It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<PayrollCalculatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRun_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var runId = Guid.NewGuid();
        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun?)null);

        var command = new CalculatePayrollCommand(runId, "system");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(PayrollStatus.Calculated)]
    [InlineData(PayrollStatus.Approved)]
    [InlineData(PayrollStatus.Paid)]
    [InlineData(PayrollStatus.Cancelled)]
    public async Task Handle_NonDraftStatus_ShouldThrowInvalidOperationException(PayrollStatus status)
    {
        // Arrange
        var runId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = Guid.NewGuid(),
            Year = 2024,
            Month = 1,
            Status = status,
            Components = new List<SalaryComponent>()
        };

        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        var command = new CalculatePayrollCommand(runId, "system");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NoConfigurationFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var runId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = Guid.NewGuid(),
            Year = 2024,
            Month = 5,
            Status = PayrollStatus.Draft,
            Components = new List<SalaryComponent>()
        };

        _runRepoMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        _configRepoMock
            .Setup(r => r.GetEffectiveConfigurationAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollConfiguration?)null);

        var command = new CalculatePayrollCommand(runId, "system");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
