using MessengerAPI.Models.DTOs;

namespace MessengerAPI.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<string> LoginAsync(LoginDto dto);
    Task<UserDto> GetCurrentUserAsync(int userId);
}
