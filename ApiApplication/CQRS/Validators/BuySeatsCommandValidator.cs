using ApiApplication.CQRS.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ApiApplication.CQRS.Validators
{
    public class BuySeatsCommandValidator : AbstractValidator<BuySeatsCommand>
    {
        public BuySeatsCommandValidator()
        {
            RuleFor(command => command.ReserveId).NotEmpty();
        }
    }
}
