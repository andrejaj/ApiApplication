using ProtoDefinitions;
using System.Threading.Tasks;

namespace ApiApplication.API
{
    public interface IApiClientGrpc
    {
        Task<showListResponse> GetAllAsync();
        Task<showResponse> GetByIdAsync(string id);
    }
}