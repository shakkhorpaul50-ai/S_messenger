using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerAPI.Data.Repositories;
using MessengerAPI.Models;

namespace MessengerAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationRepository _conversationRepository;

    public ConversationsController(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var conversations = await _conversationRepository.GetByUserIdAsync(userId);
        return Ok(conversations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = (int)HttpContext.Items["UserId"]!;

        var conversation = new Conversation
        {
            Title = request.Title,
            IsGroupChat = request.IsGroupChat,
            CreatedByUserId = userId
        };

        conversation = await _conversationRepository.CreateAsync(conversation);

        var creator = new Participant
        {
            ConversationId = conversation.Id,
            UserId = userId,
            Role = "admin"
        };
        await _conversationRepository.AddParticipantAsync(creator);

        if (request.ParticipantIds != null)
        {
            foreach (var participantId in request.ParticipantIds.Where(pid => pid != userId))
            {
                await _conversationRepository.AddParticipantAsync(new Participant
                {
                    ConversationId = conversation.Id,
                    UserId = participantId
                });
            }
        }

        conversation = await _conversationRepository.GetByIdAsync(conversation.Id);
        return Ok(conversation);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var conversation = await _conversationRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Conversation not found.");
        return Ok(conversation);
    }
}

public class CreateConversationRequest
{
    public string? Title { get; set; }
    public bool IsGroupChat { get; set; }
    public List<int>? ParticipantIds { get; set; }
}
