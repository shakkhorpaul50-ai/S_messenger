using MessengerAPI.Models.DTOs;

namespace MessengerAPI.Services;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(int id);
    Task<IEnumerable<UserDto>> SearchAsync(string query, int currentUserId);
    Task<UserDto> UpdateProfileAsync(int userId, string displayName, string? avatarUrl);
}
