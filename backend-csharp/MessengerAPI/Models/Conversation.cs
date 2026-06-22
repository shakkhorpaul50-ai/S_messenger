namespace MessengerAPI.Models;

public class Conversation
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsGroupChat { get; set; }

    public User CreatedByUser { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
