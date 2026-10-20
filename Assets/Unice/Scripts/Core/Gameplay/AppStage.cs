using System;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// The Unity side of <see cref="IStage"/> and <see cref="IMatchContext"/>: the match rules in Unice.Gameplay ask for a banner, a
    /// ripple or a metric, and this routes them to the UI, the world view, the audio and analytics.
    /// </summary>
    sealed class AppStage : IStage, IMatchContext, ISoundOutput
    {
        readonly App app;

        public AppStage(App app) { this.app = app; }

        public static Color ColorOf(StageColor tone) => tone switch
        {
            StageColor.Rose => Ui.Rose,
            StageColor.Candle => Ui.Candle,
            StageColor.Muted => Ui.Muted,
            _ => Ui.Blood,
        };

        /// <summary>The flash's color; <c>alpha</c> is how strong it is.</summary>
        static Color FlashColor(float strength) => new Color(.75f, .12f, .16f, strength);

        // ---------- IStage ----------
        public void Banner(string text, BannerStyle style, float seconds) => app.ui.Banner(text, style, seconds);
        public void Toast(string text) => app.ui.Toast(text);
        public void WorldText(float x, float y, string text, StageColor tone, float size, float seconds, bool alwaysVisible = false) =>
            app.ui.ShowWorldText(x, y, text, ColorOf(tone), size, seconds, alwaysVisible);
        public void Sound(string name) => app.sfx.Play(name);
        public void SoundFrom(float x, float y, float range, string label, StageColor tone, string sound, float minLoudness = 0) =>
            app.soundscape.PlayFrom(x, y, range, label, tone, sound, minLoudness);
        public void Ripple(float x, float y, RippleKind kind) => app.worldView.ShowRipple(x, y, kind);
        public void ShakeCamera(float magnitude) => app.cameraRig.Shake(magnitude);
        public void FlashScreen(float strength, float seconds) => app.ui.Flash(FlashColor(strength), seconds);
        public void FlashCharacter(int characterIndex) => app.worldView.FlashHit(characterIndex);
        public void StopHeartbeat() => app.heartbeat.Stop();
        public void Log(string name, params (string key, object value)[] args) => Metrics.Log(name, args);
        public void Error(Exception error, string where) => Metrics.Error(error, where);

        // ---------- ISoundOutput ----------
        public void Play(string sound, float volume, float pan, bool muffled) => app.sfx.Play(sound, volume, pan, muffled);
        public void ShowCue(float angle, float loudness, StageColor color) => app.ui.ShowSoundCue(angle, loudness, ColorOf(color));
        public void Caption(string text) => app.ui.Caption(text);

        // ---------- IMatchContext ----------
        public bool IsOnline => app.IsOnline;
        public string MyName => app.myName;
        public float Now => Time.realtimeSinceStartup;
        public bool MarkFirstMatchEnd() => app.MarkFirstMatchEnd();
        public int AddOnlineWin() => app.AddOnlineWin();
    }
}
