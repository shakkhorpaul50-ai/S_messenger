using MessengerAPI.Data.Repositories;
using MessengerAPI.Helpers;
using MessengerAPI.Models;
using MessengerAPI.Models.DTOs;

namespace MessengerAPI.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtHelper _jwtHelper;
    private readonly PasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, JwtHelper jwtHelper, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.UsernameExistsAsync(dto.Username))
            throw new InvalidOperationException("Username already exists.");

        if (await _userRepository.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email already exists.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            PasswordHash = _passwordHasher.HashPassword(dto.Password)
        };

        user = await _userRepository.CreateAsync(user);

        return MapToDto(user);
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByUsernameAsync(dto.Username)
            ?? throw new UnauthorizedAccessException("Invalid username or password.");

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return _jwtHelper.GenerateToken(user.Id, user.Username);
    }

    public async Task<UserDto> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return MapToDto(user);
    }

    private static UserDto MapToDto(User user)
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
