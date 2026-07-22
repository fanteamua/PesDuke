using System.IO;
using Newtonsoft.Json;
using PesDuke.Models;

namespace PesDuke.Services;

public class MessageQueueService
{
    private readonly Queue<ChatMessage> _queue = new();
    private readonly object _lock = new();
    private readonly string _stateFilePath;
    private string _lastReadMessageId = string.Empty;

    public int PendingCount { get; private set; }
    public int TotalProcessed { get; private set; }

    public MessageQueueService(string? stateFilePath = null)
    {
        _stateFilePath = stateFilePath ?? Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "pesduke_state.json");
        LoadState();
    }

    public void Enqueue(ChatMessage message)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(message.Id) || message.Id == _lastReadMessageId)
                return;

            _queue.Enqueue(message);
            PendingCount = _queue.Count;
        }
    }

    public ChatMessage? Dequeue()
    {
        lock (_lock)
        {
            if (_queue.Count == 0) return null;

            var msg = _queue.Dequeue();
            _lastReadMessageId = msg.Id;
            msg.IsRead = true;
            PendingCount = _queue.Count;
            TotalProcessed++;
            SaveState();
            return msg;
        }
    }

    public bool HasMessages
    {
        get { lock (_lock) { return _queue.Count > 0; } }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _queue.Clear();
            PendingCount = 0;
        }
    }

    public void ResetState()
    {
        _lastReadMessageId = string.Empty;
        TotalProcessed = 0;
        Clear();
        SaveState();
    }

    private void SaveState()
    {
        try
        {
            var state = new { LastReadMessageId = _lastReadMessageId, TotalProcessed, SavedAt = DateTime.UtcNow };
            File.WriteAllText(_stateFilePath, JsonConvert.SerializeObject(state, Formatting.Indented));
        }
        catch { /* ponytail: swallow save errors */ }
    }

    private void LoadState()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                var state = JsonConvert.DeserializeAnonymousType(
                    File.ReadAllText(_stateFilePath),
                    new { LastReadMessageId = "", TotalProcessed = 0, SavedAt = DateTime.MinValue });

                if (state != null)
                {
                    _lastReadMessageId = state.LastReadMessageId;
                    TotalProcessed = state.TotalProcessed;
                }
            }
        }
        catch { /* start fresh on load failure */ }
    }
}
