using MessengerAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MessengerAPI.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMessageService _messageService;

    public ChatHub(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task SendMessage(int conversationId, string content)
    {
        var userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var messageDto = await _messageService.SendMessageAsync(conversationId, userId, content);
        await Clients.Group($"Conversation_{conversationId}").SendAsync("MessageReceived", messageDto);
    }

    public async Task JoinConversation(int conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
    }

    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
    }

    public async Task StartTyping(int conversationId)
    {
        var username = Context.User!.FindFirst(ClaimTypes.Name)!.Value;
        await Clients.Group($"Conversation_{conversationId}").SendAsync("UserTyping", new { conversationId, username });
    }

    public async Task StopTyping(int conversationId)
    {
        var username = Context.User!.FindFirst(ClaimTypes.Name)!.Value;
        await Clients.Group($"Conversation_{conversationId}").SendAsync("UserStoppedTyping", new { conversationId, username });
    }

    public async Task MarkAsRead(int messageId, int conversationId)
    {
        var userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _messageService.MarkAsReadAsync(messageId, userId);
        await Clients.Group($"Conversation_{conversationId}").SendAsync("MessageRead", new { messageId, conversationId, userId });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
