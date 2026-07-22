using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PesDuke.Models;
using PesDuke.Services;

namespace PesDuke;

public partial class MainWindow : Window
{
    private const string PlaceholderText = "https://youtube.com/live/...";
    private bool _isPlaceholderActive = true;
    private bool _isPaused;

    // Services
    private readonly TextToSpeechService _tts = new();
    private readonly YouTubeChatService _youtube = new();
    private readonly MessageQueueService _messageQueue = new();
    private readonly SettingsService _settingsService = new();
    private readonly DispatcherTimer _speechTimer;

    public MainWindow()
    {
        InitializeComponent();

        _speechTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _speechTimer.Tick += SpeechTimer_Tick;

        // Wire events
        BtnSaveApiKey.Click += BtnSaveApiKey_Click;
        CmbVoice.SelectionChanged += CmbVoice_SelectionChanged;
        SliderSpeechRate.ValueChanged += SliderSpeechRate_ValueChanged;

        _youtube.MessageReceived += OnYouTubeMessageReceived;
        _youtube.ConnectionChanged += OnYouTubeConnectionChanged;
        _youtube.ErrorOccurred += OnYouTubeError;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        LoadSavedSettings();
        PopulateVoices();
        _speechTimer.Start();
    }

    // ═══════════════ SETTINGS ═══════════════

    private void LoadSavedSettings()
    {
        var s = _settingsService.Settings;

        if (!string.IsNullOrEmpty(s.YouTubeApiKey))
            TxtApiKey.Text = s.YouTubeApiKey;

        if (!string.IsNullOrEmpty(s.LastChannelUrl) && s.LastChannelUrl != PlaceholderText)
        {
            TxtStreamUrl.Text = s.LastChannelUrl;
            TxtStreamUrl.Foreground = (SolidColorBrush)FindResource("PrimaryTextBrush");
            _isPlaceholderActive = false;
        }

        SliderSpeechRate.Value = s.SpeechRate;
        ChkAutoStart.IsChecked = s.AutoStart;
        ChkMinimizeToTray.IsChecked = s.MinimizeToTray;

        _tts.SetRate(MapSliderToRate(s.SpeechRate));
    }

    private void PopulateVoices()
    {
        try
        {
            foreach (var voice in _tts.GetVoices())
                CmbVoice.Items.Add(voice.VoiceInfo.Name);

            if (CmbVoice.Items.Count > 0)
            {
                var saved = _settingsService.Settings.SelectedVoice;
                var idx = string.IsNullOrEmpty(saved) ? 0 :
                    CmbVoice.Items.IndexOf(saved);
                CmbVoice.SelectedIndex = idx >= 0 ? idx : 0;
            }
        }
        catch
        {
            CmbVoice.Items.Add("Default Voice");
            CmbVoice.SelectedIndex = 0;
        }
    }

    // ═══════════════ TITLE BAR ═══════════════

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else
            DragMove();
    }

    private void BtnSettings_Click(object sender, RoutedEventArgs e)
    {
        SettingsPanel.Visibility = SettingsPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed : Visibility.Visible;
    }

    private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

    // ═══════════════ URL INPUT PLACEHOLDER ═══════════════

    private void TxtStreamUrl_GotFocus(object sender, RoutedEventArgs e)
    {
        if (_isPlaceholderActive)
        {
            TxtStreamUrl.Text = string.Empty;
            TxtStreamUrl.Foreground = (SolidColorBrush)FindResource("PrimaryTextBrush");
            _isPlaceholderActive = false;
        }
    }

    private void TxtStreamUrl_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtStreamUrl.Text))
        {
            TxtStreamUrl.Text = PlaceholderText;
            TxtStreamUrl.Foreground = (SolidColorBrush)FindResource("SecondaryTextBrush");
            _isPlaceholderActive = true;
        }
    }

    // ═══════════════ CONTROL BUTTONS ═══════════════

    private void BtnStart_Click(object sender, RoutedEventArgs e)
    {
        if (_isPlaceholderActive) return;

        var url = TxtStreamUrl.Text;
        if (_isPaused)
        {
            _youtube.Resume();
            _isPaused = false;
            BtnPause.Content = "⏸ Pause";
        }
        else
        {
            BtnStart.IsEnabled = false;
            BtnPause.IsEnabled = true;
            BtnStop.IsEnabled = true;

            var apiKey = _settingsService.Settings.YouTubeApiKey;
            _ = _youtube.StartAsync(url, string.IsNullOrEmpty(apiKey) ? null : apiKey);
        }

        SetConnectionStatus(true);
    }

    private void BtnPause_Click(object sender, RoutedEventArgs e)
    {
        if (_isPaused)
        {
            _youtube.Resume();
            _isPaused = false;
            BtnPause.Content = "⏸ Pause";
            BtnStart.IsEnabled = false;
        }
        else
        {
            _youtube.Pause();
            _isPaused = true;
            BtnPause.Content = "▶ Resume";
            BtnStart.IsEnabled = true;
        }
    }

    private void BtnStop_Click(object sender, RoutedEventArgs e)
    {
        _youtube.Stop();
        _isPaused = false;
        BtnStart.IsEnabled = true;
        BtnPause.IsEnabled = false;
        BtnStop.IsEnabled = false;
        BtnPause.Content = "⏸ Pause";

        SetConnectionStatus(false);

        // Save last URL
        if (!_isPlaceholderActive)
        {
            _settingsService.Settings.LastChannelUrl = TxtStreamUrl.Text;
            _settingsService.SaveSettings(_settingsService.Settings);
        }
    }

    // ═══════════════ SETTINGS CONTROLS ═══════════════

    private void BtnSaveApiKey_Click(object sender, RoutedEventArgs e)
    {
        _settingsService.Settings.YouTubeApiKey = TxtApiKey.Text.Trim();
        _settingsService.SaveSettings(_settingsService.Settings);
    }

    private void CmbVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CmbVoice.SelectedItem is string voiceName)
        {
            _tts.SetVoice(voiceName);
            _settingsService.Settings.SelectedVoice = voiceName;
            _settingsService.SaveSettings(_settingsService.Settings);
        }
    }

    private void SliderSpeechRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;

        var rate = MapSliderToRate(e.NewValue);
        _tts.SetRate(rate);
        TxtSpeechRate.Text = $"{e.NewValue:F1}x";

        _settingsService.Settings.SpeechRate = e.NewValue;
        _settingsService.SaveSettings(_settingsService.Settings);
    }

    // Slider 0.1–3.0 → TTS rate -10..10
    private static int MapSliderToRate(double slider) => Math.Clamp((int)((slider - 1.0) * 10), -10, 10);

    // ═══════════════ SPEECH TIMER ═══════════════

    private void SpeechTimer_Tick(object? sender, EventArgs e)
    {
        if (_messageQueue.HasMessages && !_tts.IsSpeaking)
        {
            var msg = _messageQueue.Dequeue();
            if (msg != null)
            {
                msg.IsCurrentlySpeaking = true;

                // Mark previous messages as not speaking
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in ChatListBox.Items)
                    {
                        if (item is ChatMessage m && m != msg)
                            m.IsCurrentlySpeaking = false;
                    }
                    ChatListBox.Items.Refresh();
                });

                _tts.SpeakCompleted += (_, _) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        msg.IsCurrentlySpeaking = false;
                        ChatListBox.Items.Refresh();
                    });
                };

                _tts.SpeakAsync($"{msg.Author} says: {msg.Text}");
            }
        }
    }

    // ═══════════════ YOUTUBE EVENTS ═══════════════

    private void OnYouTubeMessageReceived(object? sender, ChatMessage message)
    {
        _messageQueue.Enqueue(message);
        Dispatcher.Invoke(() => AddChatMessage(message));
    }

    private void OnYouTubeConnectionChanged(object? sender, bool connected)
    {
        Dispatcher.Invoke(() => SetConnectionStatus(connected));
    }

    private void OnYouTubeError(object? sender, string error)
    {
        Dispatcher.Invoke(() =>
        {
            AddChatMessage(new ChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                Author = "System",
                Text = $"[Error] {error}",
                Timestamp = DateTime.Now
            });
        });
    }

    // ═══════════════ PUBLIC API ═══════════════

    public void AddChatMessage(ChatMessage msg)
    {
        Dispatcher.Invoke(() =>
        {
            ChatListBox.Items.Add(msg);
            EmptyState.Visibility = Visibility.Collapsed;

            var count = ChatListBox.Items.Count;
            TxtMessageCount.Text = $"{count} message{(count == 1 ? "" : "s")}";
            TxtStatusCount.Text = count.ToString();

            ChatListBox.ScrollIntoView(ChatListBox.Items[^1]);
        });
    }

    public void SetConnectionStatus(bool connected)
    {
        Dispatcher.Invoke(() =>
        {
            StatusDot.Fill = (SolidColorBrush)FindResource(connected ? "SuccessGreenBrush" : "ErrorRedBrush");
            TxtStatus.Text = connected ? "Connected" : "Disconnected";
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        _speechTimer.Stop();
        _youtube.Dispose();
        _tts.Dispose();
        base.OnClosed(e);
    }
}
