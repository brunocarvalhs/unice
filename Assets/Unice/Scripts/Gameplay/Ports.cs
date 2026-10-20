using System;

namespace Unice
{
    /// <summary>Colors of a banner (USS classes "banner-info", "banner-rose", …).</summary>
    public enum BannerStyle { Info, Rose, Blood, Moss, Candle }

    /// <summary>What made a ripple: each kind has its color, size and duration.</summary>
    public enum RippleKind
    {
        /// <summary>The doll's giggle (or a laugh): big pink ring.</summary>
        Giggle,
        /// <summary>A catch: short red ring.</summary>
        Catch,
        /// <summary>Running footsteps the Unice hears.</summary>
        Step,
        /// <summary>The player's own running steps (faint).</summary>
        Own,
    }

    /// <summary>The few colors the gameplay rules ask for by name; the UI decides the actual color.</summary>
    public enum StageColor { Rose, Candle, Muted, Blood }

    /// <summary>
    /// Everything the match rules can make the player see, hear or log, without knowing how: the UI, the world view, the
    /// audio and analytics implement it (see <c>AppStage</c> in Core). Positions are house pixels, times are seconds.
    /// </summary>
    public interface IStage
    {
        void Banner(string text, BannerStyle style, float seconds);
        void Toast(string text);
        void WorldText(float x, float y, string text, StageColor tone, float size, float seconds, bool alwaysVisible = false);
        /// <summary>A sound that is not placed in the world (a sting, a win jingle, the creak of a spot).</summary>
        void Sound(string name);
        /// <summary>A sound placed at (x, y): quieter with distance, with a cue and caption when it can't be seen. <paramref name="label"/> null = none.</summary>
        void SoundFrom(float x, float y, float range, string label, StageColor tone, string sound, float minLoudness = 0);
        void Ripple(float x, float y, RippleKind kind);
        void ShakeCamera(float magnitude);
        /// <summary>A red flash over the screen; <paramref name="strength"/> 0..1.</summary>
        void FlashScreen(float strength, float seconds);
        /// <summary>The character's sprite flashes (it was just hit).</summary>
        void FlashCharacter(int characterIndex);
        void StopHeartbeat();
        void Log(string name, params (string key, object value)[] args);
        void Error(Exception error, string where);
    }

    /// <summary>What the event feed needs to know about this device and its profile, besides the match itself.</summary>
    public interface IMatchContext
    {
        bool IsOnline { get; }
        string MyName { get; }
        /// <summary>The device clock, in seconds (used for durations).</summary>
        float Now { get; }
        /// <summary>True the first time it is called on this device (the "first match" metric).</summary>
        bool MarkFirstMatchEnd();
        /// <summary>Counts one more online win in the profile and returns the total.</summary>
        int AddOnlineWin();
    }
}
