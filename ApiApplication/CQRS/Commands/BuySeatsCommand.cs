using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Exceptions;
using ApiApplication.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.CQRS.Commands
{
    [DataContract]
    public class BuySeatsCommand : IRequest<BuySeatsDto>
    {
        [DataMember]
        public Guid ReserveId { get; private set; }

        public BuySeatsCommand(Guid reserveId)
        {
            ReserveId = reserveId;
        }
    }

    public class BuySeatsHandler : IRequestHandler<BuySeatsCommand, BuySeatsDto>
    {
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ILogger<BuySeatsHandler> _logger;

        public BuySeatsHandler(ITicketsRepository ticketsRepository, IShowtimesRepository showtimesRepository, ILogger<BuySeatsHandler> logger)
        {
            _ticketsRepository = ticketsRepository ?? throw new ArgumentNullException(nameof(ticketsRepository));
            _showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<BuySeatsDto> Handle(BuySeatsCommand command, CancellationToken cancellationToken)
        {
            var reservedTicket = await _ticketsRepository.GetAsync(command.ReserveId, cancellationToken) ?? throw new CinemaException($"Reservation id {command.ReserveId} does not exist!");

            if (reservedTicket.Paid)
            {
                throw new CinemaException($"Reservation with id {command.ReserveId} was already bought.");
            }

            var showTime = await _showtimesRepository.GetWithMoviesByIdAsync(reservedTicket.ShowtimeId, cancellationToken);

            if (reservedTicket.CreatedTime.HasExpired())
            {
                throw new CinemaException($"Reservation with id {command.ReserveId} expired.");
            }

            var confirmedTicket = await _ticketsRepository.ConfirmPaymentAsync(reservedTicket, cancellationToken);

            _logger.LogInformation($"Buying Ticket for {showTime.Movie.Title} with reservation {reservedTicket.Id}.");

            return new BuySeatsDto
            {
                TicketId = confirmedTicket.Id,
                Seats = confirmedTicket.Seats.Select(x => new SeatDto(x.Row, x.SeatNumber)),
                Movie = showTime.Movie.Title,
                SessionDate = showTime.SessionDate,
                AuditoriumId = showTime.AuditoriumId,
            };
        }
    }

    public class BuySeatsDto
    {
        public Guid TicketId { get; set; }
        public IEnumerable<SeatDto> Seats { get; set; }
        public string Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
