using MessengerAPI.Models.DTOs;

namespace MessengerAPI.Services;

public interface IMessageService
{
    Task<MessageDto> SendMessageAsync(int senderId, int conversationId, string content);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId, int skip, int take);
    Task<bool> DeleteMessageAsync(int messageId, int userId);
    Task MarkAsReadAsync(int messageId);
}
