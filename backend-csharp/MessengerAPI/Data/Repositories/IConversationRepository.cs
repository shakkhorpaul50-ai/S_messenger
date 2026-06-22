using MessengerAPI.Models;

namespace MessengerAPI.Data.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(int id);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(int userId);
    Task<Conversation> CreateAsync(Conversation conversation);
    Task<Conversation?> GetPrivateConversationAsync(int userId1, int userId2);
    Task AddParticipantAsync(Participant participant);
}
