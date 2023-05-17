using ApiApplication.CQRS.Commands;
using ApiApplication.Database.Entities;
using ApiApplication.Extensions;

namespace ApiApplicationTests
{
    public class SeatsCheckTests
    {
        private IList<SeatDto> _seats;

        [SetUp]
        public void Setup()
        {
            _seats = new List<SeatDto>() {
                new SeatDto(1, 2),
                new SeatDto(1, 3),
                new SeatDto(1, 4)
            };
        }

        [Test]
        public void SeatAreContiguous()
        {
            // Act
            var expectedResult = _seats.AreContiguous();
            
            // Assert
            Assert.That(expectedResult, Is.True);
        }

        [Test]
        public void SeatAreNotContiguous()
        {
            // Arrange
            _seats.Add(new SeatDto(1, 7));

            // Act
            var expectedResult = _seats.AreContiguous();

            // Assert
            Assert.That(expectedResult, Is.False);
        }

        [Test]
        public void SeatsAreInAuditorium()
        {
            // Arrange
            var auditorium = new AuditoriumEntity()
            {
                Id = 2,
                Seats = new List<SeatEntity>() {
                    new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 2 },
                    new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 3 },
                    new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 4 }
                }
            };

            // Act
            var expectedResult = _seats.AreInAuditorium(auditorium);

            // Assert
            Assert.That(expectedResult, Is.True);
        }

        [Test]
        public void SeatsAreNotInAuditorium()
        {
            // Arrange
            var auditorium = new AuditoriumEntity()
            {
                Id = 2,
                Seats = new List<SeatEntity>() {
                    new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 2 },
                    new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 3 }
                }
            };

            // Act
            var expectedResult = _seats.AreInAuditorium(auditorium);

            // Assert
            Assert.That(expectedResult, Is.False);
        }

        [Test]
        public void SeatsAreAvailable()
        {
            // Arrange
            var showtime = new ShowtimeEntity() {
                AuditoriumId = 2,
                SessionDate = DateTime.Now,
                Tickets = new List<TicketEntity>()
                {
                    new TicketEntity() { 
                        Id = Guid.NewGuid(), 
                        Paid = false,
                        CreatedTime = DateTime.Now,
                        Seats = new List<SeatEntity>() 
                        {
                            new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 2},
                            new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 3},
                            new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 4}
                        } 
                    }
                } 
            }; 

            // Act
            var expectedResult = _seats.AreAvailable(showtime);

            // Assert
            Assert.That(expectedResult, Is.True);
        }

        [Test]
        public void SeatsAreNotAvailable()
        {
            // Arrange
            var showtime = new ShowtimeEntity()
            {
                AuditoriumId = 2,
                SessionDate = DateTime.Now,
                Tickets = new List<TicketEntity>()
                {
                    new TicketEntity() {
                        Id = Guid.NewGuid(),
                        Paid = false,
                        CreatedTime = DateTime.Now.AddMinutes(-20),
                        Seats = new List<SeatEntity>()
                        {
                            new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 2},
                            new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 3},
                            new SeatEntity() { AuditoriumId = 2, Row = 1, SeatNumber = 4}
                        }
                    }
                }
            };

            // Act
            var expectedResult = _seats.AreAvailable(showtime);

            // Assert
            Assert.That(expectedResult, Is.False);
        }
    }
}