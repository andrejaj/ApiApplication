using ApiApplication.API;
using ApiApplication.API.DTO;
using ApiApplication.CQRS.Commands;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Internal;
using System.Linq.Expressions;

namespace ApiApplicationTests
{
    public class CreateShowtimeHandlerTests
    {
        private Mock<IApiClient> _apiclient;
        private Mock<IShowtimesRepository> _showtimesRepository;
        private Mock<ILogger<CreateShowtimeHandler>> _logger;
        private Mock<IConfiguration> _configration;

        [SetUp]
        public void Setup()
        {
            _apiclient = new Mock<IApiClient>();
            _showtimesRepository = new Mock<IShowtimesRepository>();
            _logger = new Mock<ILogger<CreateShowtimeHandler>>();
            _configration = new Mock<IConfiguration>();
        }

        [Test]
        public async Task CreateShowtimeTest()
        {
            // Arrange
            CreateShowtimeCommand command = new CreateShowtimeCommand("ID-test1", DateTime.Now, 2);
            var createShowtimeHandler = new CreateShowtimeHandler(_showtimesRepository.Object, _logger.Object, _configration.Object, delegeate => _apiclient.Object);

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
                Id = 1,
                AuditoriumId = 2,
                SessionDate = DateTime.Now,
                Movie = new MovieEntity() { Id = 2, Title = "Random Movie" },
                Tickets = new List<TicketEntity>()
                {
                    ticket
                }
            };

            ticket.Showtime = showtime;

            _showtimesRepository.Setup(x => x.GetAllAsync(It.IsAny<Expression<Func<ShowtimeEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ShowtimeEntity>());
            _showtimesRepository.Setup(x => x.CreateShowtime(It.IsAny<ShowtimeEntity>(), It.IsAny<CancellationToken>())).ReturnsAsync(showtime);

            var showDto = new Show() { Id = "2", Title = "Random Movie", Crew = "Billy Kidd", Year = "1998" };
            
            _apiclient.Setup(x => x.GetMovieAsync(It.IsAny<string>())).ReturnsAsync(showDto);
            
            // Act
            var createShowtime = await createShowtimeHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsNotNull(createShowtime);
            Assert.Multiple(() =>
            {
                Assert.That(showtime.Id, Is.EqualTo(createShowtime.Id));
                Assert.That(showtime.Movie.Id, Is.EqualTo(createShowtime.MovieId));
                Assert.That(showtime.Movie.Title, Is.EqualTo(createShowtime.MovieTitle));
                Assert.That(showtime.AuditoriumId, Is.EqualTo(createShowtime.AuditoriumId));
                Assert.That(showtime.SessionDate, Is.EqualTo(createShowtime.SessionDate));
            });
        }
    }
}
