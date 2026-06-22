using MessengerAPI.Controllers;
using MessengerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MessengerAPI.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsOk_WhenValidData()
    {
        var mockAuthService = new Mock<IAuthService>();
        var request = new RegisterRequest { Username = "newuser", Password = "Password123!", Email = "user@test.com" };
        mockAuthService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(new AuthResult { Success = true, Token = "jwt_token" });

        var controller = new AuthController(mockAuthService.Object);
        var result = await controller.Register(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUsernameTaken()
    {
        var mockAuthService = new Mock<IAuthService>();
        var request = new RegisterRequest { Username = "existing", Password = "Password123!", Email = "user@test.com" };
        mockAuthService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(new AuthResult { Success = false, ErrorMessage = "Username already taken" });

        var controller = new AuthController(mockAuthService.Object);
        var result = await controller.Register(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsToken_WhenCredentialsValid()
    {
        var mockAuthService = new Mock<IAuthService>();
        var request = new LoginRequest { Username = "testuser", Password = "correctpassword" };
        mockAuthService.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(new AuthResult { Success = true, Token = "jwt_token" });

        var controller = new AuthController(mockAuthService.Object);
        var result = await controller.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalidPassword()
    {
        var mockAuthService = new Mock<IAuthService>();
        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };
        mockAuthService.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(new AuthResult { Success = false, ErrorMessage = "Invalid credentials" });

        var controller = new AuthController(mockAuthService.Object);
        var result = await controller.Login(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
    }
}
