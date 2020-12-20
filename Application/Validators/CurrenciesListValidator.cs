using System.Collections.Generic;
using FluentValidation.Results;
using FluentValidation;
using Models;

namespace Application.Validators
{
    public class CurrenciesListValidator : AbstractValidator<IEnumerable<Currencies>>
    {
        public override ValidationResult Validate(ValidationContext<IEnumerable<Currencies>> instance)
        {
            var failures = new List<ValidationFailure>();

            if (instance == null || instance.InstanceToValidate == null)
            {
                failures.Add(new ValidationFailure("Currencies",
                        "invalid!"));
            }
            else
            {
                foreach (var employee in instance.InstanceToValidate)
                {
                    failures.AddRange(new CurrenciesValidator().Validate(employee).Errors);
                }
            }

            return new ValidationResult(failures);
        }
    }
}
