using FluentValidation;
using Models;

namespace Application.Validators
{
    public class CurrenciesValidator : AbstractValidator<Currencies>
    {
        public CurrenciesValidator()
        {
            RuleFor(p => p.Code).
                NotNull().WithMessage("'{PropertyName}' must not be empty").
                Length(1,5).WithMessage("'{PropertyName}' invalid length, it should be between '{MinLength}' and '{MaxLength}'");
            RuleFor(p => p.Name).
                NotNull().WithMessage("'{PropertyName}' must not be empty").
                Length(1, 50).WithMessage("'{PropertyName}' invalid length, it should be between '{MinLength}' and '{MaxLength}'");
        }
    }
}
