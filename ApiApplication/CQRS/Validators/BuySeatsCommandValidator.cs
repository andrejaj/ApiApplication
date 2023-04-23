using ApiApplication.CQRS.Commands;
using FluentValidation;

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
