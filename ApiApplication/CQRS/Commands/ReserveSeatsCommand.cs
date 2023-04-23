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
        public IEnumerable<SeatDto> Seats { get; private set; }

        public ReserveSeatsCommand(int showtimeId, IEnumerable<SeatDto> seats)
        {
            ShowtimeId = showtimeId;
            Seats = seats;
        }
    }

    public class SeatDto
    {
        public SeatDto(short row, short seatNumber)
        {
            Row = row;
            SeatNumber = seatNumber;
        }

        public short Row { get; set; }
        public short SeatNumber { get; set; }
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
                throw new CinemaException($"Showtime {command.ShowtimeId} does not exist!");
            }

            var auditorium = await _auditoriumsRepository.GetAsync(showtimeTickets.AuditoriumId, cancellationToken);

            _logger.LogInformation($"Checking criteria for seat reservation for showtime: {showtimeTickets.Id}");

            if (!AuditoriumHasSeats(auditorium, command.Seats))
            {
                throw new CinemaException("Seats do not exist in the auditorium!");
            }

            if (!AreAvailableSeats(showtimeTickets, command.Seats))
            {
                throw new CinemaException("Seats are not available in auditorium!");
            }

            if (!AreContiguousSeats(command.Seats))
            {
                throw new CinemaException("Seats are not contiguous in a row!");
            }

            _logger.LogInformation($"Reserving seats for showtime {showtimeTickets.Id}");

            var seats = auditorium.Seats.Where(s => command.Seats.Any(x => x.Row == s.Row && x.SeatNumber == s.SeatNumber)).ToList();
            var ticket = await _ticketsRepository.CreateAsync(showtimeTickets, seats, cancellationToken);
            var showtime = await _showtimesRepository.GetWithMoviesByIdAsync(showtimeTickets.Id, cancellationToken);

            return new ReserveSeatsDto
            {
                ReserveId = ticket.Id,
                Movie = showtime.Movie.Title,
                SeatsCount = showtime.Tickets.Select(x => x.Seats).Count(),
                AuditoriumId = ticket.Showtime.AuditoriumId,
                SessionTime = ticket.Showtime.SessionDate,
            };
        }

        private bool AreContiguousSeats(IEnumerable<SeatDto> seats)
        {
            _logger.LogInformation("Checking if all requested seats are contiguous");

            var result = seats.OrderBy(x => x.Row).ThenBy(x => x.SeatNumber).GroupWhile<SeatDto>((n1, n2) => n2.SeatNumber - n1.SeatNumber == 1);
                     
            if(result.Count() > 1)
            {
                _logger.LogInformation($"Seats are not contigious!");
                return false;
            }

            return true;
        }

        private bool AuditoriumHasSeats(AuditoriumEntity auditorium, IEnumerable<SeatDto> seats)
        {
            var hasSeats = seats.All(x => auditorium.Seats.Any(y => y.Row == x.Row && y.SeatNumber == x.SeatNumber));
            _logger.LogInformation($"Auditorium has seats {hasSeats}!");
            return hasSeats;
        }

        private bool AreAvailableSeats(ShowtimeEntity showtime, IEnumerable<SeatDto> seats)
        {   
            var availableSeats = showtime.Tickets.Where(x => !x.Paid && x.CreatedTime.HasExpired()).SelectMany(x => x.Seats).All(x => seats.Any(y => y.Row == x.Row && y.SeatNumber == x.SeatNumber));
            _logger.LogInformation($"All seats available {availableSeats}!");
            return availableSeats;
        }
    }

    public class ReserveSeatsDto
    {
        public Guid ReserveId { get; set; }
        public string Movie { get; set; }
        public int SeatsCount { get; set; }
        public int AuditoriumId { get; set; }
        public DateTime SessionTime { get; set; }
    }
}
