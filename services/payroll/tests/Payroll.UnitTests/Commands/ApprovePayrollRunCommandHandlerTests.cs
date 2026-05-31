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

public class ApprovePayrollRunCommandHandlerTests
{
    private readonly Mock<IPayrollRunRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ApprovePayrollRunCommandHandler _handler;

    public ApprovePayrollRunCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPayrollRunRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PayrollMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new ApprovePayrollRunCommandHandler(
            _repositoryMock.Object, _mapper, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_CalculatedRun_ShouldApproveSuccessfully()
    {
        // Arrange
        var runId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = Guid.NewGuid(),
            Year = 2024,
            Month = 3,
            Status = PayrollStatus.Calculated,
            GrossSalary = 2000m,
            NetSalary = 1402m,
            TotalDeductions = 598m,
            Components = new List<SalaryComponent>()
        };

        _repositoryMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        var command = new ApprovePayrollRunCommand(runId, "hr_director");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(PayrollStatus.Approved);
        payrollRun.Status.Should().Be(PayrollStatus.Approved);
        payrollRun.UpdatedBy.Should().Be("hr_director");

        _repositoryMock.Verify(r => r.UpdateAsync(payrollRun, It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(e => e.PublishAsync(It.IsAny<PayrollApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRun_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var runId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun?)null);

        var command = new ApprovePayrollRunCommand(runId, "admin");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(PayrollStatus.Draft)]
    [InlineData(PayrollStatus.Approved)]
    [InlineData(PayrollStatus.Paid)]
    [InlineData(PayrollStatus.Cancelled)]
    public async Task Handle_NonCalculatedStatus_ShouldThrowInvalidOperationException(PayrollStatus status)
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

        var command = new ApprovePayrollRunCommand(runId, "admin");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldPublishEventWithCorrectData()
    {
        // Arrange
        var runId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            EmployeeId = employeeId,
            Year = 2024,
            Month = 6,
            Status = PayrollStatus.Calculated,
            GrossSalary = 3000m,
            NetSalary = 2100m,
            TotalDeductions = 900m,
            Components = new List<SalaryComponent>()
        };

        _repositoryMock
            .Setup(r => r.GetByIdWithComponentsAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        PayrollApprovedEvent? capturedEvent = null;
        _eventPublisherMock
            .Setup(e => e.PublishAsync(It.IsAny<PayrollApprovedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<PayrollApprovedEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);

        var command = new ApprovePayrollRunCommand(runId, "hr_manager");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.PayrollRunId.Should().Be(runId);
        capturedEvent.EmployeeId.Should().Be(employeeId);
        capturedEvent.GrossSalary.Should().Be(3000m);
        capturedEvent.NetSalary.Should().Be(2100m);
        capturedEvent.ApprovedBy.Should().Be("hr_manager");
    }
}
