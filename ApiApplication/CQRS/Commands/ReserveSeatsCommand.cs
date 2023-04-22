using ApiApplication.Database.Entities;
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
    public class ReserveSeatsCommand : IRequest<ReserveSeatsDto>
    {
        [DataMember]
        public int ShowtimeId { get; private set; }

        [DataMember]
        public IEnumerable<SeatEntity> Seats { get; private set; }

        public ReserveSeatsCommand(int showtimeId, IEnumerable<SeatEntity> seats)
        {
            ShowtimeId = showtimeId;
            Seats = seats;
        }
    }

    public class ReserveSeatsHandler : IRequestHandler<ReserveSeatsCommand, ReserveSeatsDto>
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly ILogger<ReserveSeatsHandler> _logger;

        public ReserveSeatsHandler(ITicketsRepository ticketsRepository, IAuditoriumsRepository auditoriumsRepository, IShowtimesRepository showtimesRepository, ILogger<ReserveSeatsHandler> logger)
        {
            _ticketsRepository = ticketsRepository ?? throw new ArgumentNullException(nameof(ticketsRepository));
            _auditoriumsRepository = auditoriumsRepository ?? throw new ArgumentNullException(nameof(auditoriumsRepository));
            _showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ReserveSeatsDto> Handle(ReserveSeatsCommand command, CancellationToken cancellationToken)
        {
            var showtimeTickets = await _showtimesRepository.GetWithTicketsByIdAsync(command.ShowtimeId, cancellationToken);
            if (showtimeTickets == null)
            {
                throw new Exception($"Showtime {command.ShowtimeId} does not exist");
            }
            
            var auditorium = await _auditoriumsRepository.GetAsync(showtimeTickets.AuditoriumId, cancellationToken);

            _logger.LogInformation($"Pre checking criteria for seat reservation for showtime {showtimeTickets.Id}");

            if (!AuditoriumHasSeats(auditorium, command.Seats))
            {
                throw new Exception("Seats do not exist in the auditorium");
            }

            if (!AreAvailableSeats(showtimeTickets, command.Seats))
            {
                throw new Exception("Seats are not available in auditorium");
            }

            if (!AreContiguousSeats(command.Seats))
            {
                throw new Exception("Seats are not contiguous in a row!");
            }

            _logger.LogInformation($"Reserving seats for showtime {showtimeTickets.Id}");

            var ticket = await _ticketsRepository.CreateAsync(showtimeTickets, command.Seats, cancellationToken);
            var showtime = await _showtimesRepository.GetWithMoviesByIdAsync(showtimeTickets.Id, cancellationToken);

            return new ReserveSeatsDto
            {
                ReserveId = ticket.Id,
                Movie = showtime.Movie.Title,
                NumberOfSeats = command.Seats.Count(),
                AuditoriumId = ticket.Showtime.AuditoriumId,
                SessionTime = ticket.Showtime.SessionDate,
            };
        }

        private bool AreContiguousSeats(IEnumerable<SeatEntity> seats)
        {
            _logger.LogInformation("Checking if all requested seats are contiguous");
            var seatPerRow = seats.GroupBy(x => x.Row).Select(g => new { Row = g.Key, Seats = g.ToList()}).OrderBy(x => x.Seats);

            if(seatPerRow.Count() > 1)
            {
                _logger.LogInformation($"More then one row for seat!");
                throw new CinemaException("More then one row for seat!");
            }

            var consecutiveSeats = seatPerRow.FirstOrDefault()
                .Seats
                .GroupWhile<SeatEntity>((n1, n2) => n1.SeatNumber == n2.SeatNumber)
                .Select(g => new { Count = g.Count() });

            return consecutiveSeats.FirstOrDefault().Count == seats.Count();
        }

        private bool AuditoriumHasSeats(AuditoriumEntity auditorium, IEnumerable<SeatEntity> seats)
        {
            var hasSeats = seats.All(x => auditorium.Seats.Contains(x));
            _logger.LogInformation($"Auditorium has seats {hasSeats}!");
            return hasSeats;
        }

        private bool AreAvailableSeats(ShowtimeEntity showtime, IEnumerable<SeatEntity> seats)
        {
            var availableSeats = showtime.Tickets.Where(x => !x.Paid && x.CreatedTime.HasExpired()).SelectMany(x => x.Seats).All(x => seats.Contains(x));
            _logger.LogInformation($"All seats available {availableSeats}!");
            return availableSeats;
        }
    }

    public class ReserveSeatsDto
    {
        public Guid ReserveId { get; set; }
        public string Movie { get; set; }
        public int NumberOfSeats { get; set; }
        public int AuditoriumId { get; set; }
        public DateTime SessionTime { get; set; }
    }
}
