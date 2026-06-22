using Microsoft.EntityFrameworkCore;
using MessengerAPI.Models;

namespace MessengerAPI.Data.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(int id)
    {
        return await _context.Conversations
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(int userId)
    {
        return await _context.Conversations
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .ToListAsync();
    }

    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
        return conversation;
    }

    public async Task<Conversation?> GetPrivateConversationAsync(int userId1, int userId2)
    {
        return await _context.Conversations
            .Where(c => !c.IsGroupChat &&
                        c.Participants.Any(p => p.UserId == userId1) &&
                        c.Participants.Any(p => p.UserId == userId2) &&
                        c.Participants.Count == 2)
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync();
    }

    public async Task AddParticipantAsync(Participant participant)
    {
        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();
    }
}
