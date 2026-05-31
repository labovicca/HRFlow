using FluentAssertions;
using Moq;
using Payroll.Application.Commands.PayrollRuns;
using Payroll.Application.Interfaces;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.UnitTests.Commands;

public class DeletePayrollRunCommandHandlerTests
{
    private readonly Mock<IPayrollRunRepository> _repositoryMock;
    private readonly DeletePayrollRunCommandHandler _handler;

    public DeletePayrollRunCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPayrollRunRepository>();
        _handler = new DeletePayrollRunCommandHandler(_repositoryMock.Object);
    }

    [Theory]
    [InlineData(PayrollStatus.Draft)]
    [InlineData(PayrollStatus.Cancelled)]
    public async Task Handle_DraftOrCancelledRun_ShouldDeleteSuccessfully(PayrollStatus status)
    {
        // Arrange
        var runId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            Status = status
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        var command = new DeletePayrollRunCommand(runId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(runId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRun_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var runId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByIdAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun?)null);

        var command = new DeletePayrollRunCommand(runId);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(PayrollStatus.Calculated)]
    [InlineData(PayrollStatus.Approved)]
    [InlineData(PayrollStatus.Paid)]
    public async Task Handle_NonDeletableStatus_ShouldThrowInvalidOperationException(PayrollStatus status)
    {
        // Arrange
        var runId = Guid.NewGuid();
        var payrollRun = new PayrollRun
        {
            Id = runId,
            Status = status
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(runId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payrollRun);

        var command = new DeletePayrollRunCommand(runId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
