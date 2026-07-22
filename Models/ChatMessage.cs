namespace PesDuke.Models;

public class ChatMessage
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRead { get; set; }
    public bool IsCurrentlySpeaking { get; set; }
}
