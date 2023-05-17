using ApiApplication.CQRS.Commands;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiApplicationTests
{
    internal class ReserveSeatsHandlerTests
    {
        private Mock<ITicketsRepository> _ticketsRepository;
        private Mock<IAuditoriumsRepository> _auditoriumsRepository;
        private Mock<IShowtimesRepository> _showtimesRepository;
        private Mock<ILogger<ReserveSeatsHandler>> _logger;

        [SetUp]
        public void Setup()
        {
            _ticketsRepository = new Mock<ITicketsRepository>();
            _auditoriumsRepository = new Mock<IAuditoriumsRepository>();
            _showtimesRepository = new Mock<IShowtimesRepository>();
            _logger = new Mock<ILogger<ReserveSeatsHandler>>();
        }

        [Test]
        public async Task ReserveSeatsTest()
        {
            // Act
            var reserverSeatHandler = new ReserveSeatsHandler(_ticketsRepository.Object, _auditoriumsRepository.Object, _showtimesRepository.Object, _logger.Object);
            var seats = new List<SeatDto>() { new SeatDto(1, 2), new SeatDto(1, 3), new SeatDto(1, 4) };
            ReserveSeatsCommand command = new ReserveSeatsCommand(2, seats);

            var seatsEntities = new List<SeatEntity>()
            {
                new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 2},
                new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 3},
                new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 4}
            };

            var ticket = new TicketEntity()
            {
                Id = Guid.NewGuid(),
                Paid = false,
                CreatedTime = DateTime.Now,
                Seats = seatsEntities,
            };

            var showtime = new ShowtimeEntity()
            {
                AuditoriumId = 2,
                SessionDate = DateTime.Now,
                Movie = new MovieEntity() {Title = "Random Movie" },
                Tickets = new List<TicketEntity>()
                {
                    ticket
                }
            };

            ticket.Showtime = showtime;

            _showtimesRepository.Setup(x => x.GetWithTicketsByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(showtime);
            
            var auditorium = new AuditoriumEntity()
            {
                Id = 2,
                Seats = seatsEntities,
            };
            _auditoriumsRepository.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(auditorium);
            _ticketsRepository.Setup(x => x.CreateAsync(It.IsAny<ShowtimeEntity>(), It.IsAny<IEnumerable<SeatEntity>>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
            _showtimesRepository.Setup(x => x.GetWithMoviesByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(showtime);
            

            // Arrange
            var reserveSeat = await reserverSeatHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(reserveSeat);
            Assert.Multiple(() =>
            {
                Assert.That(ticket.Id, Is.EqualTo(reserveSeat.ReserveId));
                Assert.That(ticket.Showtime.AuditoriumId, Is.EqualTo(reserveSeat.AuditoriumId));
                Assert.That(ticket.Showtime.SessionDate, Is.EqualTo(reserveSeat.SessionTime));
                Assert.That(ticket.Showtime.Movie.Title, Is.EqualTo(reserveSeat.Movie));
            });
        }
    }
}
