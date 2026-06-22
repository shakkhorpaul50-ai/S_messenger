using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerAPI.Data.Repositories;
using MessengerAPI.Models.DTOs;
using MessengerAPI.Services;

namespace MessengerAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = await _authService.RegisterAsync(dto);
        var token = await _authService.LoginAsync(new LoginDto { Username = dto.Username, Password = dto.Password });
        return Ok(new { token, user });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        var dbUser = await _userRepository.GetByUsernameAsync(dto.Username);
        var user = new UserDto
        {
            Id = dbUser!.Id,
            Username = dbUser.Username,
            DisplayName = dbUser.DisplayName,
            AvatarUrl = dbUser.AvatarUrl,
            IsOnline = dbUser.IsOnline,
            LastSeenAt = dbUser.LastSeenAt
        };
        return Ok(new { token, user });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var user = await _authService.GetCurrentUserAsync(userId);
        return Ok(user);
    }
}
