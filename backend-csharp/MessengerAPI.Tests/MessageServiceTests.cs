using MessengerAPI.Repositories;
using MessengerAPI.Services;
using Moq;
using Xunit;

namespace MessengerAPI.Tests;

public class MessageServiceTests
{
    [Fact]
    public async Task SendMessage_ReturnsMessageDto_WhenValid()
    {
        var mockMessageRepo = new Mock<IMessageRepository>();
        var mockConversationRepo = new Mock<IConversationRepository>();

        mockConversationRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        mockMessageRepo.Setup(r => r.AddAsync(It.IsAny<Message>())).ReturnsAsync(new MessageDto
        {
            Id = 1,
            Content = "Hello",
            SenderId = 1,
            ConversationId = 1
        });

        var service = new MessageService(mockMessageRepo.Object, mockConversationRepo.Object);
        var result = await service.SendMessageAsync(1, 1, "Hello");

        Assert.NotNull(result);
        Assert.Equal("Hello", result.Content);
    }

    [Fact]
    public async Task GetMessages_ReturnsPaginatedMessages()
    {
        var mockMessageRepo = new Mock<IMessageRepository>();
        var mockConversationRepo = new Mock<IConversationRepository>();

        var messages = new List<MessageDto>
        {
            new() { Id = 1, Content = "First", SenderId = 1, ConversationId = 1 },
            new() { Id = 2, Content = "Second", SenderId = 2, ConversationId = 1 }
        };

        mockMessageRepo.Setup(r => r.GetByConversationIdAsync(1, 1, 20)).ReturnsAsync(messages);

        var service = new MessageService(mockMessageRepo.Object, mockConversationRepo.Object);
        var result = await service.GetMessagesAsync(1, 1, 20);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteMessage_ReturnsTrue_WhenOwnMessage()
    {
        var mockMessageRepo = new Mock<IMessageRepository>();
        var mockConversationRepo = new Mock<IConversationRepository>();

        mockMessageRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MessageDto
        {
            Id = 1,
            Content = "Test",
            SenderId = 1,
            ConversationId = 1
        });
        mockMessageRepo.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        var service = new MessageService(mockMessageRepo.Object, mockConversationRepo.Object);
        var result = await service.DeleteMessageAsync(1, 1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteMessage_ReturnsFalse_WhenNotOwnMessage()
    {
        var mockMessageRepo = new Mock<IMessageRepository>();
        var mockConversationRepo = new Mock<IConversationRepository>();

        mockMessageRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MessageDto
        {
            Id = 1,
            Content = "Test",
            SenderId = 2,
            ConversationId = 1
        });

        var service = new MessageService(mockMessageRepo.Object, mockConversationRepo.Object);
        var result = await service.DeleteMessageAsync(1, 1);

        Assert.False(result);
    }
}
