using ApiApplication.API.DTO;
using ProtoDefinitions;
using System.Threading.Tasks;

namespace ApiApplication.API
{
    public interface IApiClient
    {
        //Task<showListResponse> GetAllMoviesAsync();
        Task<Show> GetMovieAsync(string id);
        //Task<showResponse> SearchMovieAsync(SearchRequest request);
    }
}