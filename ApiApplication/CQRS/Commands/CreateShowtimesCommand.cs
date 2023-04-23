using ApiApplication.API;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Exceptions;
using ApiApplication.Helper;
using MediatR;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.CQRS.Commands
{
    [DataContract]
    public class CreateShowtimeCommand : IRequest<ShowtimeDto>
    {
        [DataMember]
        public int MovieId { get; private set; }

        [DataMember]
        public DateTime SessionDate { get; private set; }

        [DataMember]
        public int AuditoriumId { get; private set; }

        public CreateShowtimeCommand(int movieId, DateTime sessionDate, int auditoriumId)
        {
            MovieId = movieId;
            SessionDate = sessionDate;
            AuditoriumId = auditoriumId;
        }
    }

    public class CreateShowtimeHandler : IRequestHandler<CreateShowtimeCommand, ShowtimeDto>
    {
        private readonly IApiClientGrpc _apiclient;
        private readonly IShowtimesRepository _showtimesRepository;
        private readonly ILogger<CreateShowtimeHandler> _logger;

        public CreateShowtimeHandler(IApiClientGrpc apiClient, IShowtimesRepository showtimesRepository, ILogger<CreateShowtimeHandler> logger)
        {
            _apiclient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ShowtimeDto> Handle(CreateShowtimeCommand message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Creating Showtime for Movie {message.MovieId}");

            // showtime exists we shouldn't add second time
            var showTimeExists = await _showtimesRepository.GetAllAsync(FilterByMovieVenue(message.MovieId, message.AuditoriumId, message.SessionDate), cancellationToken);
            if (showTimeExists.Any())
            {
                throw new CinemaException($"Showtime found for MovieId {message.MovieId}, AuditoriumId {message.AuditoriumId} and SessionDate {message.SessionDate}.");
            }

            var movieDetails = await _apiclient.GetMovieAsync(message.MovieId.ToString());

            var showtime = new ShowtimeEntity
            {
                SessionDate = message.SessionDate,
                AuditoriumId = message.AuditoriumId,
                Movie = new MovieEntity
                {
                    Title = movieDetails.Title,
                    ImdbId = movieDetails.Id,
                    ReleaseDate = new DateTime(int.Parse(movieDetails.Year), RandomValue.GetRandom(12), RandomValue.GetRandom(28)),
                    Stars = movieDetails.Crew,
                },
            };

            showtime = await _showtimesRepository.CreateShowtime(showtime, cancellationToken);

            _logger.LogInformation($"Showtime {showtime.Id} created for Movie {movieDetails.Id}");

            return ShowtimeDto.ConvertFrom(showtime);
        }

        Expression<Func<ShowtimeEntity, bool>> FilterByMovieVenue(int MovieId, int AuditoriumId, DateTime sessionDate)
        {
            return x => x.Movie.Id == MovieId && x.AuditoriumId == AuditoriumId && x.SessionDate.Equals(sessionDate);
        }
    }

    public record ShowtimeDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }

        public static ShowtimeDto ConvertFrom(ShowtimeEntity showtime)
        {
            return new ShowtimeDto()
            {
                Id = showtime.Id,
                MovieId = showtime.Movie.Id,
                MovieTitle = showtime.Movie.Title,
                SessionDate = showtime.SessionDate,
                AuditoriumId = showtime.AuditoriumId,
            };
        }
    }
}
