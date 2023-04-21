using ApiApplication.CQRS.Commands;
using ApiApplication.Database.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;

namespace ApiApplication.CQRS.Validators
{
    public class ReserveSeatsCommandValidator : AbstractValidator<ReserveSeatsCommand>
    {
        public ReserveSeatsCommandValidator()
        {
            RuleFor(command => command.ShowtimeId).NotEmpty();
            RuleFor(command => command.Seats).NotEmpty();
            RuleFor(command => command.Seats).Must(HaveSeats).WithMessage("No seats");
        }

        private bool HaveSeats(IEnumerable<SeatEntity> seats) => seats.Any();
    }
}
