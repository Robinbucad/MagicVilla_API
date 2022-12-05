using MagicVilla_Web.Models;
using MagicVilla_WEB.Models;

namespace MagicVilla_WEB.Services.IServices
{
    public interface IBaseService
    {
        APIResponse responseModel { get; set; }
        Task<T> SendAsync<T>(APIRequest apiRequest);
    }
}
