using MessengerAPI.Models;

namespace MessengerAPI.Data.Repositories;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(int id);
    Task<IEnumerable<Message>> GetByConversationIdAsync(int conversationId, int skip, int take);
    Task<Message> CreateAsync(Message message);
    Task<bool> DeleteAsync(int id);
    Task<bool> SoftDeleteAsync(int id);
    Task MarkAsReadAsync(int messageId);
    Task<int> GetUnreadCountAsync(int userId, int conversationId);
}
