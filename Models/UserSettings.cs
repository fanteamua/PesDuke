namespace PesDuke.Models;

public class UserSettings
{
    public string YouTubeApiKey { get; set; } = string.Empty;
    public string SelectedVoice { get; set; } = string.Empty;
    public double SpeechRate { get; set; } = 1.0;
    public bool AutoStart { get; set; }
    public string LastChannelUrl { get; set; } = string.Empty;
    public bool MinimizeToTray { get; set; } = true;
}
