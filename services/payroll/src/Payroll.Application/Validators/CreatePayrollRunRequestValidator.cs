using FluentValidation;
using Payroll.Application.DTOs;

namespace Payroll.Application.Validators;

public class CreatePayrollRunRequestValidator : AbstractValidator<CreatePayrollRunRequest>
{
    public CreatePayrollRunRequestValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("Employee ID je obavezan.");

        RuleFor(x => x.Year)
            .InclusiveBetween(2020, 2100).WithMessage("Godina mora biti između 2020 i 2100.");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Mesec mora biti između 1 i 12.");

        RuleForEach(x => x.AdditionalComponents)
            .SetValidator(new CreateSalaryComponentRequestValidator())
            .When(x => x.AdditionalComponents != null && x.AdditionalComponents.Any());
    }
}

public class CreateSalaryComponentRequestValidator : AbstractValidator<CreateSalaryComponentRequest>
{
    public CreateSalaryComponentRequestValidator()
    {
        RuleFor(x => x.ComponentType)
            .NotEmpty().WithMessage("Tip komponente je obavezan.")
            .Must(t => t.Equals("Addition", StringComparison.OrdinalIgnoreCase) || 
                       t.Equals("Deduction", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Tip komponente mora biti 'Addition' ili 'Deduction'.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv komponente je obavezan.")
            .MaximumLength(100).WithMessage("Naziv komponente ne može biti duži od 100 karaktera.");

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0).WithMessage("Iznos mora biti pozitivan.")
            .When(x => !x.Percentage.HasValue);

        RuleFor(x => x.Percentage)
            .InclusiveBetween(0, 1).WithMessage("Procenat mora biti između 0 i 1 (npr. 0.15 = 15%).")
            .When(x => x.Percentage.HasValue);
    }
}
