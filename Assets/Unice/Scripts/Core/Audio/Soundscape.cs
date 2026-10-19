using System.Collections.Generic;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// What this player hears from the house: sounds placed in the world (quieter with distance, panned, muffled through
    /// walls, with a cue and a caption when the source can't be seen), footsteps, the Unice's cane, the clock, and the
    /// ticking in the last seconds of a round.
    /// </summary>
    sealed class Soundscape
    {
        /// <summary>Seconds between two footstep sounds of a runner, and between two taps of the Unice's cane.</summary>
        const float StepInterval = .42f, CaneInterval = .45f;
        /// <summary>How far the tick of the map's clock (<see cref="Map.Clock"/>) carries.</summary>
        const float ClockRange = 260;
        /// <summary>The last seconds of a round tick.</summary>
        const int CountdownTickFrom = 10;

        readonly App app;
        float ownStepTimer, clockTimer;
        int lastTickSecond;
        /// <summary>Per character index: seconds until its next footstep sound.</summary>
        readonly Dictionary<int, float> stepTimers = new Dictionary<int, float>();

        public Soundscape(App app) { this.app = app; }

        /// <summary>
        /// Plays <paramref name="sound"/> as heard from (x, y): silent beyond <paramref name="range"/> (30% more while hidden),
        /// louder closer, panned, muffled through walls. If the source can't be seen, also shows a cue pointing at it and a
        /// caption describing it (for everyone in the round, hidden or not: the giggle is game information, and not everyone
        /// can hear it). Returns the loudness (0..1).
        /// </summary>
        /// <param name="label">What the caption calls it ("passos correndo"); null = no cue or caption.</param>
        /// <param name="minLoudness">Never quieter than this (for sounds the whole house must hear).</param>
        public float PlayFrom(float x, float y, float range, string label, Color cueColor, string sound, float minLoudness = 0)
        {
            var perspective = app.perspective;
            if (!perspective.hasView) return 0;
            var me = app.myCharacter; var listener = perspective.view; bool hidden = me != null && me.hidingSpot != null;
            float reach = SoundMath.Reach(range, hidden), distance = Geometry.Distance(x, y, listener.x, listener.y);
            float loudness = SoundMath.Loudness(distance, reach, minLoudness);
            if (loudness <= SoundMath.AudibleLoudness) return 0;

            bool throughWall = SoundMath.IsMuffled(distance, Map.HasLineOfSight(listener.x, listener.y, x, y));
            app.sfx.Play(sound, loudness, SoundMath.Pan(x - listener.x), throughWall);
            if (label != null && me != null && !me.outOfRound && !perspective.CanSee(x, y))
            {
                app.ui.ShowSoundCue(Mathf.Atan2(y - listener.y, x - listener.x), loudness, cueColor);
                app.ui.Caption($"{label} · {SoundMath.DescribeDistance(loudness)} · {SoundMath.DescribeSide(x - listener.x, y - listener.y)}");
            }
            return loudness;
        }

        /// <summary>Per step while a round is live and not paused.</summary>
        public void Tick(float dt)
        {
            var game = app.game; var me = app.myCharacter;
            foreach (var character in game.characters)
            {
                if (character == me || character.outOfRound || character.hidingSpot != null || !character.moving || character.x < 0) continue;
                stepTimers.TryGetValue(character.index, out float timer);
                timer -= dt; stepTimers[character.index] = timer;
                if (timer > 0) continue;
                if (character.IsUnice)
                {
                    // the Unice's cane: only the team hears it (the Unice's own player knows where it is)
                    stepTimers[character.index] = CaneInterval;
                    if (app.perspective.ShowsTeamInfo) PlayFrom(character.x, character.y, 380, Localization.Get("sound.cane"), Ui.Rose, "cane");
                }
                else if (character.loud)
                {
                    stepTimers[character.index] = StepInterval;
                    float loudness = PlayFrom(character.x, character.y, Rules.NoiseRange, Localization.Get("sound.running"), Ui.Candle, "step");
                    if (loudness > 0 && me != null && me.IsUnice) app.worldView.ShowRipple(character.x, character.y, RippleKind.Step);
                }
            }

            // your own running shows as faint ripples, so you know you're being loud
            if (me != null && me.loud)
            {
                ownStepTimer -= dt;
                if (ownStepTimer <= 0) { ownStepTimer = StepInterval; app.worldView.ShowRipple(me.x, me.y, RippleKind.Own); }
            }

            var clock = Map.Clock;
            clockTimer -= dt;
            if (clock != null && clockTimer <= 0) { clockTimer = 1; PlayFrom(clock.CenterX, clock.CenterY, ClockRange, null, default, "clock"); }

            if (game.phase == Phase.Hunt)
            {
                int secondsLeft = Mathf.CeilToInt(game.timeLeft);
                if (secondsLeft <= CountdownTickFrom && secondsLeft > 0 && secondsLeft != lastTickSecond) { lastTickSecond = secondsLeft; app.sfx.Play("tick"); }
            }
        }
    }
}
