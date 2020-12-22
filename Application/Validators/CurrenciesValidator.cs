using Infrastructure.Resources.Validations;
using FluentValidation;
using Models;

namespace Application.Validators
{
    public class CurrenciesValidator : AbstractValidator<Currencies>
    {
        public CurrenciesValidator()
        {
            RuleFor(p => p.Code).
                NotNull().WithMessage(ValidationMessages.Mandatory).
                Length(1,5).WithMessage(ValidationMessages.InvalidLength);
            RuleFor(p => p.Name).
                NotNull().WithMessage(ValidationMessages.Mandatory).
                Length(1, 50).WithMessage(ValidationMessages.InvalidLength);
        }
    }
}
