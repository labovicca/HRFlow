using AutoMapper;
using FluentAssertions;
using Moq;
using Payroll.Application.Commands.PayrollRuns;
using Payroll.Application.DTOs;
using Payroll.Application.Interfaces;
using Payroll.Application.Mappings;
using Payroll.Domain.Entities;
using Payroll.Domain.Enums;

namespace Payroll.UnitTests.Commands;

public class CreatePayrollRunCommandHandlerTests
{
    private readonly Mock<IPayrollRunRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly CreatePayrollRunCommandHandler _handler;

    public CreatePayrollRunCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPayrollRunRepository>();
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PayrollMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new CreatePayrollRunCommandHandler(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreatePayrollRun()
    {
        // Arrange
        var request = new CreatePayrollRunRequest
        {
            EmployeeId = Guid.NewGuid(),
            Year = 2024,
            Month = 6
        };

        _repositoryMock
            .Setup(r => r.GetByEmployeeAndPeriodAsync(request.EmployeeId, request.Year, request.Month, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun?)null);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<PayrollRun>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun pr, CancellationToken _) =>
            {
                pr.Id = Guid.NewGuid();
                pr.CreatedAt = DateTime.UtcNow;
                return pr;
            });

        var command = new CreatePayrollRunCommand(request, "admin");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EmployeeId.Should().Be(request.EmployeeId);
        result.Year.Should().Be(2024);
        result.Month.Should().Be(6);
        result.Status.Should().Be(PayrollStatus.Draft);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<PayrollRun>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicatePayrollRun_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new CreatePayrollRunRequest
        {
            EmployeeId = employeeId,
            Year = 2024,
            Month = 6
        };

        _repositoryMock
            .Setup(r => r.GetByEmployeeAndPeriodAsync(employeeId, 2024, 6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PayrollRun { Id = Guid.NewGuid(), EmployeeId = employeeId });

        var command = new CreatePayrollRunCommand(request, "admin");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAdditionalComponents_ShouldAddComponents()
    {
        // Arrange
        var request = new CreatePayrollRunRequest
        {
            EmployeeId = Guid.NewGuid(),
            Year = 2024,
            Month = 3,
            AdditionalComponents = new List<CreateSalaryComponentRequest>
            {
                new()
                {
                    ComponentType = "Addition",
                    Name = "Bonus",
                    Amount = 500m,
                    IsTaxable = true
                },
                new()
                {
                    ComponentType = "Deduction",
                    Name = "Kredit",
                    Amount = 100m,
                    IsTaxable = false
                }
            }
        };

        _repositoryMock
            .Setup(r => r.GetByEmployeeAndPeriodAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun?)null);

        PayrollRun? capturedPayrollRun = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<PayrollRun>(), It.IsAny<CancellationToken>()))
            .Callback<PayrollRun, CancellationToken>((pr, _) => capturedPayrollRun = pr)
            .ReturnsAsync((PayrollRun pr, CancellationToken _) =>
            {
                pr.Id = Guid.NewGuid();
                pr.CreatedAt = DateTime.UtcNow;
                return pr;
            });

        var command = new CreatePayrollRunCommand(request, "admin");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPayrollRun.Should().NotBeNull();
        capturedPayrollRun!.Components.Should().HaveCount(2);
        capturedPayrollRun.Components.Should().Contain(c => c.Name == "Bonus" && c.ComponentType == ComponentType.Addition);
        capturedPayrollRun.Components.Should().Contain(c => c.Name == "Kredit" && c.ComponentType == ComponentType.Deduction);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedByFromCommand()
    {
        // Arrange
        var request = new CreatePayrollRunRequest
        {
            EmployeeId = Guid.NewGuid(),
            Year = 2024,
            Month = 1
        };

        _repositoryMock
            .Setup(r => r.GetByEmployeeAndPeriodAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PayrollRun?)null);

        PayrollRun? capturedRun = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<PayrollRun>(), It.IsAny<CancellationToken>()))
            .Callback<PayrollRun, CancellationToken>((pr, _) => capturedRun = pr)
            .ReturnsAsync((PayrollRun pr, CancellationToken _) =>
            {
                pr.Id = Guid.NewGuid();
                pr.CreatedAt = DateTime.UtcNow;
                return pr;
            });

        var command = new CreatePayrollRunCommand(request, "hr_manager");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedRun.Should().NotBeNull();
        capturedRun!.CreatedBy.Should().Be("hr_manager");
    }
}
