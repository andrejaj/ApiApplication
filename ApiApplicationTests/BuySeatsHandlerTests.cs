using ApiApplication.CQRS.Commands;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ApiApplicationTests
{
    public class BuySeatsHandlerTests
    {
        private Mock<ITicketsRepository> _ticketsRepository;
        private Mock<IShowtimesRepository> _showtimesRepository;
        private Mock<ILogger<BuySeatsHandler>> _logger;

        [SetUp]
        public void Setup()
        {
            _ticketsRepository = new Mock<ITicketsRepository>();
            _showtimesRepository = new Mock<IShowtimesRepository>();
            _logger = new Mock<ILogger<BuySeatsHandler>>();
        }

        [Test]
        public async Task BuySeatsTest()
        {
            // Arrange
            BuySeatsCommand command = new BuySeatsCommand(Guid.NewGuid());

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
                Movie = new MovieEntity() { Title = "Random Movie" },
                Tickets = new List<TicketEntity>()
                {
                    ticket
                }
            };

            ticket.Showtime = showtime;

            _ticketsRepository.Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
            _showtimesRepository.Setup(x => x.GetWithMoviesByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(showtime);
            _ticketsRepository.Setup(x => x.ConfirmPaymentAsync(It.IsAny<TicketEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
                        
            var buySeatsHandler = new BuySeatsHandler(_ticketsRepository.Object, _showtimesRepository.Object, _logger.Object);

            // Act
            var buySeats = await buySeatsHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(buySeats);
            Assert.Multiple(() =>
            {
                Assert.That(ticket.Id, Is.EqualTo(buySeats.TicketId));
                Assert.That(ticket.Showtime.AuditoriumId, Is.EqualTo(buySeats.AuditoriumId));
                Assert.That(buySeats.Status, Is.EqualTo("Payment Processed"));
                Assert.That(ticket.Showtime.SessionDate, Is.EqualTo(buySeats.SessionDate));
            });
        }
    }
}