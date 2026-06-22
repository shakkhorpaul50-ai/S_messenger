using Microsoft.EntityFrameworkCore;
using MessengerAPI.Models;

namespace MessengerAPI.Data.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(int id)
    {
        return await _context.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Message>> GetByConversationIdAsync(int conversationId, int skip, int take)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    public async Task<Message> CreateAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null) return false;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null) return false;

        message.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCountAsync(int userId, int conversationId)
    {
        return await _context.Messages
            .Where(m => m.ConversationId == conversationId && m.SenderId != userId && !m.IsRead)
            .CountAsync();
    }
}
