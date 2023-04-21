using System;

namespace ApiApplication.Exceptions
{
    public class CinemaException : Exception
    {
        public string[] Errors { get; set; } = Array.Empty<string>();

        public CinemaException()
        { }

        public CinemaException(string message)
            : base(message)
        { }

        public CinemaException(string message, string[] errors)
        : base(message)
        {
            Errors = errors;
        }

        public CinemaException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
