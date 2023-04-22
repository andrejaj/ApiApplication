using ApiApplication.CQRS.Commands;
using ApiApplication.Database.Repositories;
using ApiApplication.Database.Repositories.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CinemaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CinemaController> _logger;

        public CinemaController(IMediator mediator, ILogger<CinemaController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("create")]
        [HttpPost]
        public async Task<ActionResult<ShowtimeDto>> CreateShowtimeAsync([FromBody] CreateShowtimeCommand createShowtimeCommand)
        {
            _logger.LogInformation($"[Sending command]:{nameof(createShowtimeCommand)} -  MovieId: {createShowtimeCommand.MovieId}");
            var response =  await _mediator.Send(createShowtimeCommand);
            //return response;
            return response == null ? NotFound() : Ok(response);
        }

        [Route("reserve")]
        [HttpPut]
        public async Task<ActionResult<ReserveSeatsDto>> ReserveSeatsAsync([FromBody] ReserveSeatsCommand reserveSeatsCommand)
        {
            _logger.LogInformation($"[Sending command]:{nameof(reserveSeatsCommand)} - Showtime Id: {reserveSeatsCommand.ShowtimeId}");
            var response = await _mediator.Send(reserveSeatsCommand);
            return response == null ? NotFound() : Ok(response);
        }

        [Route("buyseats")]
        [HttpPut]
        public async Task<ActionResult<BuySeatsDto>> BuySeatsAsync([FromBody] BuySeatsCommand buySeatsCommand)
        {
            _logger.LogInformation($"[Sending command]:{nameof(buySeatsCommand)} - Reservation Id: {buySeatsCommand.ReserveId}");
            var response =  await _mediator.Send(buySeatsCommand);
            return response == null ? NotFound() : Ok(response);
        }
    }
}
