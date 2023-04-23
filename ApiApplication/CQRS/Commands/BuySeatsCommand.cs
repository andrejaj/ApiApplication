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
            var reservedTicket = await _ticketsRepository.GetAsync(command.ReserveId, cancellationToken) ?? throw new CinemaException($"Invalid Reservation id {command.ReserveId}!");

            if (reservedTicket.Paid)
            {
                throw new CinemaException($"Reservation {command.ReserveId} was already bought.");
            }

            if (reservedTicket.CreatedTime.HasExpired())
            {
                throw new CinemaException($"Reservation {command.ReserveId} has expired.");
            }

            var showTime = await _showtimesRepository.GetWithMoviesByIdAsync(reservedTicket.ShowtimeId, cancellationToken);
            var confirmedTicket = await _ticketsRepository.ConfirmPaymentAsync(reservedTicket, cancellationToken);

            _logger.LogInformation($"Buying Ticket for Movie {showTime.Movie.Title} with reservation {reservedTicket.Id}.");

            var seats = confirmedTicket.Seats?.Select(s => new SeatDto(s.Row, s.SeatNumber)).ToList();

            return new BuySeatsDto
            {
                TicketId = confirmedTicket.Id,
                Seats = seats,
                Movie = showTime.Movie.Title,
                SessionDate = showTime.SessionDate,
                AuditoriumId = showTime.AuditoriumId,
                Status = "Payment Processed"
            };
        }
    }


    public class BuySeatsDto
    {
        public Guid TicketId { get; set; }
        public IList<SeatDto> Seats { get; set; }
        public string Movie { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
        public string Status { get; set; }
    }
}
