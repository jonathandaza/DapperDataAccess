using FluentValidation;
using Models;

namespace Application.Validators
{
    public class CurrenciesValidator : AbstractValidator<Currencies>
    {
        public CurrenciesValidator()
        {
            RuleFor(p => p.Id).NotNull().NotEmpty();
            RuleFor(p => p.Code).
                NotNull().WithMessage("Code must not be empty").
                Length(1,5).WithMessage("invalid length");
            RuleFor(p => p.Name).
                NotNull().WithMessage("Name must not be empty").
                Length(1, 5).WithMessage("invalid length");
        }
    }
}
