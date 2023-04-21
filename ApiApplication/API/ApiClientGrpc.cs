using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProtoDefinitions;

namespace ApiApplication.API
{
    internal class ApiClientGrpc : IApiClientGrpc
    {
        private readonly MoviesApi.MoviesApiClient client;
        private readonly GrpcChannel channel;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ApiClientGrpc> _logger;
        private readonly string APIKEY;

        public ApiClientGrpc(IDistributedCache cache, ILogger<ApiClientGrpc> logger, IConfiguration configuration)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            APIKEY = configuration["CinemaApi:ApiKey"];
            if (string.IsNullOrEmpty(APIKEY)) throw new ArgumentNullException(nameof(APIKEY), "CinemaApi key not found");

            var address = configuration["CinemaApi:BaseUrl"];
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException(nameof(address), "CinemaApi url address not found");

            // Create a channel to the gRPC server
            channel = CreateAuthenticatedChannel(address);

            // Create a client for the gRPC service
            client = new MoviesApi.MoviesApiClient(channel);
        }

        public async Task<showListResponse> GetAllAsync()
        {
            var all = await client.GetAllAsync(new Empty());
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public Task<showResponse> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<showResponse> GetShowtimeAsync(string id)
        {
            var response = await client.GetByIdAsync(new IdRequest { Id = id });
            if (response.Success)
            {
                _logger.LogInformation("Fetching movie from GRPC service - Movie: {@Id}", id);
                response.Data.TryUnpack<showResponse>(out var data);
                await _cache.SetRecordAsync(id, data, absoluteExpireTime: TimeSpan.FromMinutes(5), slidingExpireTime: TimeSpan.FromMinutes(10));
                return data;
            }
            else
            {
                _logger.LogInformation("GRPC service failed, trying from cache..");
                var cached = await _cache.GetRecordAsync<showResponse>(id);
                if (cached != null)
                {
                    _logger.LogInformation("Movie found in cache}");
                    return cached;
                }
                else
                {
                    _logger.LogError("Movie {@Id} Not found", id);
                    throw new Exception($"MovieId {id} Not found");
                }
            }
        }

        private GrpcChannel CreateAuthenticatedChannel(string address)
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(APIKEY))
                {
                    metadata.Add("X-Apikey", $"{APIKEY}");
                }
                return Task.CompletedTask;
            });

            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
                HttpHandler = httpHandler
            });
            return channel;
        }
    }
}