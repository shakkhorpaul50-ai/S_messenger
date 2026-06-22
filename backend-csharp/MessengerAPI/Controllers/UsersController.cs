using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerAPI.Services;

namespace MessengerAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        return Ok(user);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var users = await _userService.SearchAsync(query, userId);
        return Ok(users);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var user = await _userService.UpdateProfileAsync(userId, request.DisplayName, request.AvatarUrl);
        return Ok(user);
    }
}

public class UpdateProfileRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
