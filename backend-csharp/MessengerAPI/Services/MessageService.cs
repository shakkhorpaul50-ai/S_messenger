using MessengerAPI.Data.Repositories;
using MessengerAPI.Models;
using MessengerAPI.Models.DTOs;

namespace MessengerAPI.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;

    public MessageService(IMessageRepository messageRepository, IConversationRepository conversationRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
    }

    public async Task<MessageDto> SendMessageAsync(int senderId, int conversationId, string content)
    {
        var conversation = await _conversationRepository.GetByIdAsync(conversationId)
            ?? throw new KeyNotFoundException("Conversation not found.");

        if (!conversation.Participants.Any(p => p.UserId == senderId))
            throw new UnauthorizedAccessException("User is not a participant of this conversation.");

        var sender = await _userRepository.GetByIdAsync(senderId)
            ?? throw new KeyNotFoundException("Sender not found.");

        var message = new Message
        {
            SenderId = senderId,
            ConversationId = conversationId,
            Content = content
        };

        message = await _messageRepository.CreateAsync(message);

        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = sender.DisplayName,
            SenderAvatar = sender.AvatarUrl,
            ConversationId = message.ConversationId,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            IsOwn = true
        };
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, int userId, int skip, int take)
    {
        var messages = await _messageRepository.GetByConversationIdAsync(conversationId, skip, take);

        return messages.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.Sender.DisplayName,
            SenderAvatar = m.Sender.AvatarUrl,
            ConversationId = m.ConversationId,
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead,
            IsOwn = m.SenderId == userId
        });
    }

    public async Task<bool> DeleteMessageAsync(int messageId, int userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId)
            ?? throw new KeyNotFoundException("Message not found.");

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("You can only delete your own messages.");

        return await _messageRepository.SoftDeleteAsync(messageId);
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        await _messageRepository.MarkAsReadAsync(messageId);
    }
}
