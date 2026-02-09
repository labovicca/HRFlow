using AutoMapper;
using FluentAssertions;
using Moq;
using Payroll.Application.Commands.PayrollRuns;
using Payroll.Application.Events;
using Payroll.Application.Interfaces;
using Payroll.Application.Mappings;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.UnitTests.Commands;

public class MarkPayrollAsPaidCommandHandlerTests
{
    private readonly Mock<IPayrollRunRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly MarkPayrollAsPaidCommandHandler _handler;

    public MarkPayrollAsPaidCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPayrollRunRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PayrollMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new MarkPayrollAsPaidCommandHandler(
            _repositoryMock.Object, _mapper, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_ApprovedRun_ShouldMarkAsPaidSuccessfully()
    {
        // Arrange
        var runId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = Guid.NewGuid(),
            Year = 2026,
            Month = 1,
            Status = PayrollStatus.Approved,
            GrossSalary = 120000m,
            NetSalary = 96060m,
            TotalDeductions = 23940m,
            Components = new List<SalaryComponent>()
        };

        _repositoryMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        var command = new MarkPayrollAsPaidCommand(runId, "finance_admin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PayrollStatus.Paid);
        payrollRun.Status.Should().Be(PayrollStatus.Paid);
        payrollRun.UpdatedBy.Should().Be("finance_admin");

        _repositoryMock.Verify(r => r.UpdateAsync(payrollRun, It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<PayrollPaidEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRun_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var runId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun?)null);

        var command = new MarkPayrollAsPaidCommand(runId, "admin");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(PayrollStatus.Draft)]
    [InlineData(PayrollStatus.Calculated)]
    [InlineData(PayrollStatus.Paid)]
    [InlineData(PayrollStatus.Cancelled)]
    public async Task Handle_NonApprovedStatus_ShouldThrowInvalidOperationException(PayrollStatus status)
    {
        // Arrange
        var runId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = Guid.NewGuid(),
            Status = status,
            Components = new List<SalaryComponent>()
        };

        _repositoryMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        var command = new MarkPayrollAsPaidCommand(runId, "admin");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldPublishPaidEventWithCorrectData()
    {
        // Arrange
        var runId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = employeeId,
            Year = 2026,
            Month = 2,
            Status = PayrollStatus.Approved,
            NetSalary = 85000m,
            Components = new List<SalaryComponent>()
        };

        _repositoryMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        PayrollPaidEvent? capturedEvent = null;
        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<PayrollPaidEvent>(), It.IsAny<CancellationToken>()))
            .Callback<PayrollPaidEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        var command = new MarkPayrollAsPaidCommand(runId, "cfo");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.PayrollRunId.Should().Be(runId);
        capturedEvent.EmployeeId.Should().Be(employeeId);
        capturedEvent.NetSalary.Should().Be(85000m);
        capturedEvent.PaidBy.Should().Be("cfo");
    }
}
