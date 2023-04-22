using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace ApiApplication.PiplineBehvaiours
{
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            //Request
            _logger.LogInformation($"[START] Handling {typeof(TRequest).Name}");
            var stopwatch = Stopwatch.StartNew();
            TResponse response;

            try
            {
                try
                {
                    var requestData = JsonSerializer.Serialize(request);
                    _logger.LogInformation($"[DATA]: {requestData}");
                }
                catch (Exception)
                {
                    _logger.LogInformation("[Serialization ERROR] Could not serialize the request.");
                }
                response = await next();
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    $"[STOP] Handling {typeof(TResponse).Name}; Execution time = {stopwatch.ElapsedMilliseconds}ms");
            }

            return response;
        }
    }
}
