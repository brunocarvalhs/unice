using System;
using System.Collections.Generic;
using System.Linq;

namespace Unice
{
    /// <summary>Records what the rules asked the UI, the audio and analytics to do, so tests can read it back.</summary>
    public sealed class FakeStage : IStage, IMatchContext
    {
        public readonly List<(string text, BannerStyle style, float seconds)> Banners = new List<(string, BannerStyle, float)>();
        public readonly List<string> Toasts = new List<string>();
        public readonly List<string> Sounds = new List<string>();
        public readonly List<(float x, float y, string label, string sound)> PlacedSounds = new List<(float, float, string, string)>();
        public readonly List<(float x, float y, string text)> WorldTexts = new List<(float, float, string)>();
        public readonly List<RippleKind> Ripples = new List<RippleKind>();
        public readonly List<float> Shakes = new List<float>();
        public readonly List<(float strength, float seconds)> Flashes = new List<(float, float)>();
        public readonly List<int> HitFlashes = new List<int>();
        public readonly List<(string name, (string key, object value)[] args)> Logs = new List<(string, (string, object)[])>();
        public readonly List<string> Errors = new List<string>();
        public int HeartbeatStops;
        /// <summary>Makes <see cref="Toast"/> throw, to test that one broken event does not stop the others.</summary>
        public bool ThrowOnToast;

        public bool IsOnline { get; set; }
        public string MyName { get; set; } = "Ana";
        public float Now { get; set; }
        public bool FirstMatch { get; set; }
        public int OnlineWins { get; private set; }

        public void Banner(string text, BannerStyle style, float seconds) => Banners.Add((text, style, seconds));
        public void Toast(string text)
        {
            if (ThrowOnToast) throw new InvalidOperationException("broken toast");
            Toasts.Add(text);
        }
        public void WorldText(float x, float y, string text, StageColor tone, float size, float seconds, bool alwaysVisible = false) => WorldTexts.Add((x, y, text));
        public void Sound(string name) => Sounds.Add(name);
        public void SoundFrom(float x, float y, float range, string label, StageColor tone, string sound, float minLoudness = 0) => PlacedSounds.Add((x, y, label, sound));
        public void Ripple(float x, float y, RippleKind kind) => Ripples.Add(kind);
        public void ShakeCamera(float magnitude) => Shakes.Add(magnitude);
        public void FlashScreen(float strength, float seconds) => Flashes.Add((strength, seconds));
        public void FlashCharacter(int characterIndex) => HitFlashes.Add(characterIndex);
        public void StopHeartbeat() => HeartbeatStops++;
        public void Log(string name, params (string key, object value)[] args) => Logs.Add((name, args));
        public void Error(Exception error, string where) => Errors.Add(where);
        public bool MarkFirstMatchEnd() => FirstMatch;
        public int AddOnlineWin() => ++OnlineWins;

        public IEnumerable<string> LogNames => Logs.Select(entry => entry.name);
        public object LogValue(string name, string key) => Logs.First(entry => entry.name == name).args.First(arg => arg.key == key).value;
    }
}
