using ApiApplication.API.DTO;
using ApiApplication.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ApiApplication.API
{
    internal class ApiClientHttp : IApiClient
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<ApiClientHttp> _logger;
        private readonly HttpClient _httpClient;
       
        public ApiClientHttp(IDistributedCache cache, ILogger<ApiClientHttp> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            var apikey = configuration["CinemaApi:ApiKey"];
            if (string.IsNullOrEmpty(apikey)) throw new ArgumentNullException(nameof(apikey), "APIKEY not found!");

            var address = configuration["CinemaApi:BaseUrlHttp"];
            if (string.IsNullOrEmpty(address)) throw new ArgumentNullException(nameof(address), "Url address not found!");

            _httpClient.BaseAddress = new Uri(address);
            _httpClient.DefaultRequestHeaders.Add("X-Apikey", apikey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Show> GetMovieAsync(string id)
        {
            _logger.LogInformation($"Getting movie {id} via Http request!");
            var response = await _httpClient.GetAsync( $"/v1/movies/{id}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var show = await response.Content.ReadFromJsonAsync<Show>();
                await _cache.SetRecordAsync(id, show, absoluteExpireTime: TimeSpan.FromMinutes(5), slidingExpireTime: TimeSpan.FromMinutes(10));
                return show;
            }
            else
            {
                _logger.LogInformation($"Failed to retrive a movie {id} via Http request!");
                var cached = await _cache.GetRecordAsync<Show>(id);
                if (cached == null)
                {
                    throw new CinemaException($"MovieId {id} not found in cache!");
                }
                return cached;
            }
        }
    }
}
