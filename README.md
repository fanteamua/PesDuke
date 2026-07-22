# 🐕 PesDuke

**YouTube Live Chat → Speech (TTS) reader for streamers**

PesDuke reads your YouTube live chat messages aloud using text-to-speech. Paste a live stream URL, click Start, and your chat is read in real time — perfect for streamers who want to interact with chat without looking at the screen.

---

## Features

- **Real-time chat reading** — polls YouTube Live Chat API and reads new messages as they arrive
- **Text-to-Speech** — uses built-in Windows voices (male/female, adjustable speed)
- **Dark theme** — custom dark UI with yellow-blue accent colors
- **State persistence** — remembers last read message, resuming from where you left off after restart
- **Settings panel** — configure YouTube API key, voice, speech rate, auto-start, minimize to tray
- **Compact installer** — single .exe or Inno Setup installer

---

## Screenshots

> _Coming soon — run the app to see the dark-themed UI_

---

## How It Works

1. **Paste a YouTube live URL** (e.g. `https://youtube.com/live/ABC123`)
2. **Click ▶ Start** — PesDuke connects to YouTube Live Chat API
3. **Messages appear** in the chat panel and are read aloud via TTS
4. **Pause/Stop** at any time — state is saved automatically
5. **Restart** — picks up from the last read message

### Architecture

```
┌─────────────────────────────────────────────┐
│  MainWindow.xaml  (WPF UI)                 │
├─────────────────────────────────────────────┤
│  TextToSpeechService  — Windows TTS engine  │
│  YouTubeChatService   — API polling         │
│  MessageQueueService  — queue + persistence  │
│  SettingsService      — JSON settings        │
└─────────────────────────────────────────────┘
```

---

## Getting PesDuke

### Option 1: Download installer (easiest)

1. Go to [Releases](https://github.com/fanteamua/PesDuke/releases)
2. Download `PesDuke-Setup-v1.0.exe`
3. Run the installer
4. Launch PesDuke from Start Menu or desktop

### Option 2: Download .exe only

1. Go to [Releases](https://github.com/fanteamua/PesDuke/releases)
2. Download `PesDuke.exe`
3. Run it — no installation needed (self-contained, ~160 MB)

---

## Build from Source

### Prerequisites

- **Windows 10/11**
- **.NET SDK 8.0** — [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** (optional) — [Download](https://git-scm.com)

### Quick build (one command)

```batch
build_all.bat
```

This builds the self-contained .exe and (if Inno Setup is installed) the installer.

### Manual build

```batch
:: Clone the repo
git clone https://github.com/fanteamua/PesDuke.git
cd PesDuke

:: Build .exe
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish

:: Output: publish\PesDuke.exe
```

### Build installer (optional)

1. Install [Inno Setup 6](https://jrsoftware.org/isinfo.php)
2. Open `installer\pesduke_setup.iss` in Inno Setup
3. Click **Build → Compile**
4. Output: `installer\output\PesDuke-Setup-v1.0.exe`

---

## YouTube API Key

PesDuke includes a built-in default API key, but for reliable use you should get your own:

1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Create a project (or use existing)
3. Enable **YouTube Data API v3**
4. Go to **Credentials → Create Credentials → API Key**
5. Copy the key into PesDuke **Settings → YouTube API Key → Save**

> _Free tier: 10,000 units/day. Live chat polling uses ~200 units/hour._

---

## Settings

| Setting | Description |
|---------|-------------|
| **YouTube API Key** | Your personal API key (optional, built-in key available) |
| **Voice** | TTS voice (male/female, depends on installed Windows voices) |
| **Speech Rate** | 0.1x to 3.0x speed |
| **Auto-start on launch** | Start reading automatically when PesDuke opens |
| **Minimize to tray** | Minimize to system tray instead of taskbar |

---

## Project Structure

```
PesDuke/
├── MainWindow.xaml          # UI layout (dark theme, custom chrome)
├── MainWindow.xaml.cs       # Code-behind (service wiring, events)
├── App.xaml                 # Resource loading (themes)
├── Models/
│   ├── ChatMessage.cs       # Message data model
│   └── UserSettings.cs      # Settings data model
├── Services/
│   ├── TextToSpeechService.cs    # Windows TTS wrapper
│   ├── YouTubeChatService.cs     # YouTube Live Chat polling
│   ├── MessageQueueService.cs    # Thread-safe queue + persistence
│   └── SettingsService.cs        # JSON settings load/save
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   └── BoolToSpeakingBgConverter.cs
├── Resources/
│   ├── AccentColors.xaml    # Color/brush definitions
│   └── DarkTheme.xaml       # Styled controls
├── installer/
│   └── pesduke_setup.iss    # Inno Setup script
└── build_all.bat            # Build script
```

---

## Tech Stack

- **C# / WPF** — .NET 8.0
- **System.Speech** — Windows built-in TTS (no external dependencies)
- **Newtonsoft.Json** — settings/state persistence
- **YouTube Data API v3** — live chat polling

---

## License

MIT License — do whatever you want with it.

---

## Author

Made by **fante** — [@fanteamua](https://github.com/fanteamua)
