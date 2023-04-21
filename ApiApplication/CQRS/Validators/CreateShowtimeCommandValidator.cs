using ApiApplication.CQRS.Commands;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;

namespace ApiApplication.CQRS.Validators
{
    public class CreateShowtimeCommandValidator : AbstractValidator<CreateShowtimeCommand>
    {
        public CreateShowtimeCommandValidator()
        {
            RuleFor(command => command.MovieId).NotEmpty();
            RuleFor(command => command.AuditoriumId).NotEmpty();
            RuleFor(command => command.SessionDate).NotEmpty().Must(HaveValidDate).WithMessage("Please specify a valid date!");
        }

        private bool HaveValidDate(DateTime dateTime) => dateTime >= DateTime.UtcNow;
    }
}
