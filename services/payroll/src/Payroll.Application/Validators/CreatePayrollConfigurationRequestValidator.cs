using FluentValidation;
using Payroll.Application.DTOs;

namespace Payroll.Application.Validators;

public class CreatePayrollConfigurationRequestValidator : AbstractValidator<CreatePayrollConfigurationRequest>
{
    public CreatePayrollConfigurationRequestValidator()
    {
        RuleFor(x => x.BaseSalary)
            .GreaterThan(0).WithMessage("Osnovna plata mora biti veća od 0.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa poreza mora biti između 0 i 1 (npr. 0.10 = 10%).");

        RuleFor(x => x.PensionContributionRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa PIO mora biti između 0 i 1.");

        RuleFor(x => x.HealthInsuranceRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa zdravstvenog mora biti između 0 i 1.");

        RuleFor(x => x.UnemploymentInsuranceRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa nezaposlenosti mora biti između 0 i 1.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Datum početka važenja je obavezan.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .WithMessage("Datum isteka mora biti nakon datuma početka.")
            .When(x => x.EffectiveTo.HasValue);
    }
}

public class UpdatePayrollConfigurationRequestValidator : AbstractValidator<UpdatePayrollConfigurationRequest>
{
    public UpdatePayrollConfigurationRequestValidator()
    {
        RuleFor(x => x.BaseSalary)
            .GreaterThan(0).WithMessage("Osnovna plata mora biti veća od 0.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa poreza mora biti između 0 i 1.");

        RuleFor(x => x.PensionContributionRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa PIO mora biti između 0 i 1.");

        RuleFor(x => x.HealthInsuranceRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa zdravstvenog mora biti između 0 i 1.");

        RuleFor(x => x.UnemploymentInsuranceRate)
            .InclusiveBetween(0, 1).WithMessage("Stopa nezaposlenosti mora biti između 0 i 1.");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("Datum početka važenja je obavezan.");

        RuleFor(x => x.EffectiveTo)
            .GreaterThan(x => x.EffectiveFrom)
            .WithMessage("Datum isteka mora biti nakon datuma početka.")
            .When(x => x.EffectiveTo.HasValue);
    }
}
