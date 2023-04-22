using ProtoDefinitions;
using System.Threading.Tasks;

namespace ApiApplication.API
{
    public interface IApiClientGrpc
    {
        Task<showListResponse> GetAllMoviesAsync();
        Task<showResponse> GetMovieAsync(string id);
    }
}