using System.Speech.Synthesis;

namespace PesDuke.Services;

public class TextToSpeechService : IDisposable
{
    private readonly SpeechSynthesizer _synth;

    public TextToSpeechService()
    {
        _synth = new SpeechSynthesizer();
        _synth.SetOutputToDefaultAudioDevice();
    }

    public List<InstalledVoice> GetVoices() => _synth.GetInstalledVoices().ToList();

    public void SetVoice(string voiceName)
    {
        var voice = _synth.GetInstalledVoices()
            .FirstOrDefault(v => v.VoiceInfo.Name == voiceName);
        if (voice != null)
            _synth.SelectVoice(voiceName);
    }

    public void SetRate(int rate) => _synth.Rate = rate;

    public void SetVolume(int volume) => _synth.Volume = Math.Clamp(volume, 0, 100);

    public void SpeakAsync(string text) => _synth.SpeakAsync(text);

    public void SpeakAsyncCancelAll() => _synth.SpeakAsyncCancelAll();

    public bool IsSpeaking => _synth.State == SynthesizerState.Speaking;

    public event EventHandler<SpeakCompletedEventArgs>? SpeakCompleted
    {
        add => _synth.SpeakCompleted += value;
        remove => _synth.SpeakCompleted -= value;
    }

    public void Dispose()
    {
        _synth?.Dispose();
        GC.SuppressFinalize(this);
    }
}
