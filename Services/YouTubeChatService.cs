using System.Net.Http;
using Newtonsoft.Json.Linq;
using PesDuke.Models;

namespace PesDuke.Services;

public class YouTubeChatService : IDisposable
{
    private readonly HttpClient _http;
    private Timer? _pollTimer;
    private string _liveChatId = string.Empty;
    private string _nextPageToken = string.Empty;
    private string _apiKey = string.Empty;
    private bool _isRunning;

    private const string DEFAULT_API_KEY = "AIzaSyBdVJcFhZQFkF3B6B5J5B5B5B5B5B5B5B";

    public event EventHandler<ChatMessage>? MessageReceived;
    public event EventHandler<bool>? ConnectionChanged;
    public event EventHandler<string>? ErrorOccurred;

    public YouTubeChatService()
    {
        _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public static string? ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        url = url.Trim();

        if (url.Contains("youtube.com/live/"))
            return url.Split("youtube.com/live/").Last().Split('?')[0].Split('/')[0];

        if (url.Contains("youtube.com/watch"))
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }

        if (url.Contains("youtu.be/"))
            return url.Split("youtu.be/").Last().Split('?')[0];

        if (url.Length == 11 && url.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            return url;

        return null;
    }

    public async Task StartAsync(string streamUrl, string? customApiKey = null)
    {
        _apiKey = !string.IsNullOrWhiteSpace(customApiKey) ? customApiKey : DEFAULT_API_KEY;

        var videoId = ExtractVideoId(streamUrl);
        if (string.IsNullOrEmpty(videoId))
        {
            ErrorOccurred?.Invoke(this, "Invalid YouTube URL");
            return;
        }

        _liveChatId = await GetLiveChatIdAsync(videoId);
        if (string.IsNullOrEmpty(_liveChatId))
        {
            ErrorOccurred?.Invoke(this, "Could not find live chat. Stream may not be live.");
            return;
        }

        _isRunning = true;
        _nextPageToken = string.Empty;
        ConnectionChanged?.Invoke(this, true);

        _pollTimer = new Timer(async _ => await PollChatAsync(), null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
    }

    public void Stop()
    {
        _isRunning = false;
        _pollTimer?.Change(Timeout.Infinite, 0);
        _pollTimer?.Dispose();
        _pollTimer = null;
        _liveChatId = string.Empty;
        _nextPageToken = string.Empty;
        ConnectionChanged?.Invoke(this, false);
    }

    public void Pause()
    {
        _pollTimer?.Change(Timeout.Infinite, 0);
    }

    public void Resume()
    {
        if (_isRunning && !string.IsNullOrEmpty(_liveChatId))
            _pollTimer?.Change(TimeSpan.Zero, TimeSpan.FromSeconds(3));
    }

    private async Task<string> GetLiveChatIdAsync(string videoId)
    {
        try
        {
            var url = $"https://www.googleapis.com/youtube/v3/videos?part=liveStreamingDetails&id={videoId}&key={_apiKey}";
            var response = await _http.GetStringAsync(url);
            var json = JObject.Parse(response);

            var items = json["items"]?.FirstOrDefault();
            return items?["liveStreamingDetails"]?["activeLiveChatId"]?.ToString() ?? string.Empty;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error getting live chat: {ex.Message}");
            return string.Empty;
        }
    }

    private async Task PollChatAsync()
    {
        if (!_isRunning || string.IsNullOrEmpty(_liveChatId)) return;

        try
        {
            var url = $"https://www.googleapis.com/youtube/v3/liveChat/messages?part=snippet,authorDetails&liveChatId={_liveChatId}&key={_apiKey}";
            if (!string.IsNullOrEmpty(_nextPageToken))
                url += $"&pageToken={_nextPageToken}";

            var response = await _http.GetStringAsync(url);
            var json = JObject.Parse(response);

            _nextPageToken = json["nextPageToken"]?.ToString() ?? string.Empty;

            var items = json["items"];
            if (items != null)
            {
                foreach (var item in items)
                {
                    var snippet = item["snippet"];
                    var author = item["authorDetails"];

                    if (snippet == null || author == null) continue;

                    var message = new ChatMessage
                    {
                        Id = item["id"]?.ToString() ?? Guid.NewGuid().ToString(),
                        Author = author["displayName"]?.ToString() ?? "Unknown",
                        Text = snippet["displayMessage"]?.ToString() ?? string.Empty,
                        Timestamp = DateTime.Parse(snippet["publishedAt"]?.ToString() ?? DateTime.UtcNow.ToString("o")),
                        IsRead = false
                    };

                    MessageReceived?.Invoke(this, message);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Poll error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
        _http?.Dispose();
        GC.SuppressFinalize(this);
    }
}
