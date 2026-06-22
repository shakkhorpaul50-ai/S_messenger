using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerAPI.Services;

namespace MessengerAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("conversation/{conversationId}")]
    public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var messages = await _messageService.GetMessagesAsync(conversationId, userId, skip, take);
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var message = await _messageService.SendMessageAsync(userId, request.ConversationId, request.Content);
        return Ok(message);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var result = await _messageService.DeleteMessageAsync(id, userId);
        return Ok(new { deleted = result });
    }
}

public class SendMessageRequest
{
    public int ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
}
