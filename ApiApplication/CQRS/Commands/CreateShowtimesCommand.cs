using MediatR;
using System.Runtime.Serialization;
using System;
using ApiApplication.Database.Entities;
using ApiApplication.Database.Repositories.Abstractions;
using Microsoft.Extensions.Logging;
using ProtoDefinitions;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using ApiApplication.Exceptions;
using ApiApplication.API;

namespace ApiApplication.CQRS.Commands
{
    public class CreateShowtimeCommand : IRequest<ShowtimeDto>
    {
        [DataMember]
        public string MovieId { get; private set; }

        [DataMember]
        public DateTime SessionDate { get; private set; }

        [DataMember]
        public int AuditoriumId { get; private set; }

        public CreateShowtimeCommand(string movieId, DateTime screeningDate, int auditoriumId)
        {
            MovieId = movieId;
            SessionDate = screeningDate;
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
            // Validate if showtime already exists
            //TODO: We should validate if the showtime exists in the database for the given movie, auditorium and session date

            var showTimeExists = await _showtimesRepository.GetAllAsync(FilterByMovieVenue(int.Parse(message.MovieId), message.AuditoriumId, message.SessionDate), cancellationToken);
            if (!showTimeExists.Any())
            {
                new CinemaException($"Showtime not found for MovieId {message.MovieId} with AuditoriumId {message.AuditoriumId} and SessionDate {message.SessionDate}");
            }

            var movieDetails = await _apiclient.GetByIdAsync(message.MovieId);

            var showtime = new ShowtimeEntity
            {
                SessionDate = message.SessionDate,
                AuditoriumId = message.AuditoriumId,
                Movie = new MovieEntity
                {
                    Title = movieDetails.Title,
                    ImdbId = movieDetails.Id,
                    ReleaseDate = new DateTime(int.Parse(movieDetails.Year), 1, 1),
                    Stars = movieDetails.Crew,
                },
            };

            _logger.LogInformation($"Creating Showtime for Movie: {movieDetails.Title}");

            showtime = await _showtimesRepository.CreateShowtime(showtime, cancellationToken);

            return ShowtimeDto.Convert(showtime);
        }

        Expression<Func<ShowtimeEntity, bool>> FilterByMovieVenue(int MovieId, int AuditoriumId, DateTime sessionDate)
        {
            return x => x.Movie.Id == MovieId && x.AuditoriumId == AuditoriumId && x.SessionDate.Equals(sessionDate);
        }
    }

    public class ShowtimeDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public string MovieTitle { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }

        public static ShowtimeDto Convert(ShowtimeEntity showtime)
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
