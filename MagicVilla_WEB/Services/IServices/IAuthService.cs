using MagicVilla_Web.Models.Dto;
using MagicVilla_WEB.Models.Dto;

namespace MagicVilla_WEB.Services.IServices
{
    public interface IAuthService
    {
        Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
        Task<T> RegisterAsync<T>(RegistrationRequestDTO objToCreate);
    }
}
