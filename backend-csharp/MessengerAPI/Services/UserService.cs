using MessengerAPI.Data.Repositories;
using MessengerAPI.Models.DTOs;

namespace MessengerAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        return MapToDto(user);
    }

    public async Task<IEnumerable<UserDto>> SearchAsync(string query, int currentUserId)
    {
        var users = await _userRepository.SearchAsync(query);
        return users
            .Where(u => u.Id != currentUserId)
            .Select(MapToDto);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, string displayName, string? avatarUrl)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.DisplayName = displayName;
        user.AvatarUrl = avatarUrl;

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    private static UserDto MapToDto(Models.User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            IsOnline = user.IsOnline,
            LastSeenAt = user.LastSeenAt
        };
    }
}
