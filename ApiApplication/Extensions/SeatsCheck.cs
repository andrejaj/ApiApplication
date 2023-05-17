using ApiApplication.CQRS.Commands;
using ApiApplication.Database.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace ApiApplication.Extensions
{
    public static class SeatsCheck
    {
        public static bool AreContiguous(this IEnumerable<SeatDto> seats)
        {
            var result = seats.OrderBy(x => x.Row).ThenBy(x => x.SeatNumber).GroupWhile<SeatDto>((n1, n2) => n2.SeatNumber - n1.SeatNumber == 1);
            return !(result.Count() > 1);
        }

        public static bool AreInAuditorium(this IEnumerable<SeatDto> seats, AuditoriumEntity auditorium)
        {
            return seats.All(x => auditorium.Seats.Any(y => y.Row == x.Row && y.SeatNumber == x.SeatNumber));
        }

        public static bool AreAvailable(this IEnumerable<SeatDto> seats, ShowtimeEntity showtime)
        {
            var avilableSeats = showtime.Tickets.Where(x => (!x.Paid && !x.CreatedTime.HasExpired())).SelectMany(x => x.Seats).ToList();
            if (avilableSeats.Any())
            {
                var query = from s1 in seats
                            join s2 in avilableSeats on new { s1.Row, s1.SeatNumber } equals new { s2.Row, s2.SeatNumber }
                            select s1;
                return query.Any();
            }

            return false;
        }
    }
}
