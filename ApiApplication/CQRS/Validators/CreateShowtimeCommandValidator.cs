using ApiApplication.CQRS.Commands;
using FluentValidation;
using System;

namespace ApiApplication.CQRS.Validators
{
    public class CreateShowtimeCommandValidator : AbstractValidator<CreateShowtimeCommand>
    {
        public CreateShowtimeCommandValidator()
        {
            RuleFor(command => command.MovieId).NotEmpty();
            RuleFor(command => command.AuditoriumId).NotEmpty();
            RuleFor(command => command.SessionDate).NotEmpty().Must(HaveValidDate).WithMessage("Valid date required!");
        }

        private bool HaveValidDate(DateTime dateTime) => dateTime >= DateTime.UtcNow;
    }
}
