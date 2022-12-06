using MagicVilla_WEB.Models.Dto;

namespace MagicVilla_Web.Models.Dto
{
    public class LoginResponseDTO
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
    }
}
