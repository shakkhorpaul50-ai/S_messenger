using MessengerAPI.Hubs;
using MessengerAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MessengerAPI.Tests;

public class ChatHubTests
{
    [Fact]
    public async Task SendMessage_BroadcastsToGroup()
    {
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        var mockContext = new Mock<HubCallerContext>();
        var mockMessageService = new Mock<IMessageService>();

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        mockContext.Setup(c => c.User).Returns(principal);
        mockClients.Setup(c => c.Group("Conversation_1")).Returns(mockClientProxy.Object);
        mockMessageService.Setup(s => s.SendMessageAsync(1, 1, "hello"))
            .ReturnsAsync(new MessageDto { Id = 1, Content = "hello", SenderId = 1, ConversationId = 1 });

        var hub = new ChatHub(mockMessageService.Object);
        hub.Clients = mockClients.Object;
        hub.Context = mockContext.Object;

        await hub.SendMessage(1, "hello");

        mockClientProxy.Verify(
            p => p.SendAsync("MessageReceived", It.IsAny<object>(), default),
            Times.Once);
    }

    [Fact]
    public async Task JoinConversation_AddsToGroup()
    {
        var mockGroups = new Mock<IGroupManager>();
        var mockContext = new Mock<HubCallerContext>();
        var mockMessageService = new Mock<IMessageService>();

        mockContext.Setup(c => c.ConnectionId).Returns("connection1");
        mockGroups.Setup(g => g.AddToGroupAsync("connection1", "Conversation_1", default))
            .Returns(Task.CompletedTask);

        var hub = new ChatHub(mockMessageService.Object);
        hub.Context = mockContext.Object;
        hub.Groups = mockGroups.Object;

        await hub.JoinConversation(1);

        mockGroups.Verify(
            g => g.AddToGroupAsync("connection1", "Conversation_1", default),
            Times.Once);
    }
}
