using System;
using System.Collections.Generic;

namespace Unice
{
    /// <summary>Where <see cref="Soundscape"/> sends what it computed: the speakers, the direction cue and the caption.</summary>
    public interface ISoundOutput
    {
        /// <param name="volume">0..1.</param>
        /// <param name="pan">-1 (left) .. 1 (right).</param>
        /// <param name="muffled">Heard through a wall.</param>
        void Play(string sound, float volume, float pan, bool muffled);
        /// <summary>An arrow at the screen edge pointing at a sound the player can't see.</summary>
        void ShowCue(float angle, float loudness, StageColor color);
        void Caption(string text);
    }

    /// <summary>
    /// What this player hears from the house: sounds placed in the world (quieter with distance, panned, muffled through
    /// walls, with a cue and a caption when the source can't be seen), footsteps, the Unice's cane, the clock, and the
    /// ticking in the last seconds of a round.
    /// </summary>
    public sealed class Soundscape
    {
        /// <summary>Seconds between two footstep sounds of a runner, and between two taps of the Unice's cane.</summary>
        const float StepInterval = .42f, CaneInterval = .45f;
        /// <summary>How far the tick of the map's clock (<see cref="Map.Clock"/>) carries, and how often it ticks (seconds).</summary>
        const float ClockRange = 260, ClockInterval = 1;
        /// <summary>How far the Unice's cane carries (house pixels).</summary>
        const float CaneRange = 380;
        /// <summary>The last seconds of a round tick.</summary>
        const int CountdownTickFrom = 10;

        readonly Game game;
        readonly Func<Character> myCharacter;
        readonly Perspective perspective;
        readonly IStage stage;
        readonly ISoundOutput output;
        float ownStepTimer, clockTimer;
        int lastTickSecond;
        /// <summary>Per character index: seconds until its next footstep sound.</summary>
        readonly Dictionary<int, float> stepTimers = new Dictionary<int, float>();

        public Soundscape(Game game, Func<Character> myCharacter, Perspective perspective, IStage stage, ISoundOutput output)
        {
            this.game = game; this.myCharacter = myCharacter; this.perspective = perspective; this.stage = stage; this.output = output;
        }

        /// <summary>
        /// Plays <paramref name="sound"/> as heard from (x, y): silent beyond <paramref name="range"/> (30% more while hidden),
        /// louder closer, panned, muffled through walls. If the source can't be seen, also shows a cue pointing at it and a
        /// caption describing it (for everyone in the round, hidden or not: the giggle is game information, and not everyone
        /// can hear it). Returns the loudness (0..1).
        /// </summary>
        /// <param name="label">What the caption calls it ("passos correndo"); null = no cue or caption.</param>
        /// <param name="minLoudness">Never quieter than this (for sounds the whole house must hear).</param>
        public float PlayFrom(float x, float y, float range, string label, StageColor cueColor, string sound, float minLoudness = 0)
        {
            if (!perspective.hasView) return 0;
            var me = myCharacter(); var listener = perspective.view; bool hidden = me != null && me.hidingSpot != null;
            float reach = SoundMath.Reach(range, hidden), distance = Geometry.Distance(x, y, listener.x, listener.y);
            float loudness = SoundMath.Loudness(distance, reach, minLoudness);
            if (loudness <= SoundMath.AudibleLoudness) return 0;

            bool throughWall = SoundMath.IsMuffled(distance, Map.HasLineOfSight(listener.x, listener.y, x, y));
            output.Play(sound, loudness, SoundMath.Pan(x - listener.x), throughWall);
            if (label != null && me != null && !me.outOfRound && !perspective.CanSee(x, y))
            {
                output.ShowCue(MathF.Atan2(y - listener.y, x - listener.x), loudness, cueColor);
                output.Caption($"{label} · {SoundMath.DescribeDistance(loudness)} · {SoundMath.DescribeSide(x - listener.x, y - listener.y)}");
            }
            return loudness;
        }

        /// <summary>Per step while a round is live and not paused.</summary>
        public void Tick(float dt)
        {
            var me = myCharacter();
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
                    if (perspective.ShowsTeamInfo) PlayFrom(character.x, character.y, CaneRange, Localization.Get("sound.cane"), StageColor.Rose, "cane");
                }
                else if (character.loud)
                {
                    stepTimers[character.index] = StepInterval;
                    float loudness = PlayFrom(character.x, character.y, Rules.NoiseRange, Localization.Get("sound.running"), StageColor.Candle, "step");
                    if (loudness > 0 && me != null && me.IsUnice) stage.Ripple(character.x, character.y, RippleKind.Step);
                }
            }

            // your own running shows as faint ripples, so you know you're being loud
            if (me != null && me.loud)
            {
                ownStepTimer -= dt;
                if (ownStepTimer <= 0) { ownStepTimer = StepInterval; stage.Ripple(me.x, me.y, RippleKind.Own); }
            }

            var clock = Map.Clock;
            clockTimer -= dt;
            if (clock != null && clockTimer <= 0) { clockTimer = ClockInterval; PlayFrom(clock.CenterX, clock.CenterY, ClockRange, null, default, "clock"); }

            if (game.phase == Phase.Hunt)
            {
                int secondsLeft = (int)Math.Ceiling(game.timeLeft);
                if (secondsLeft <= CountdownTickFrom && secondsLeft > 0 && secondsLeft != lastTickSecond) { lastTickSecond = secondsLeft; output.Play("tick", 1, 0, false); }
            }
        }
    }
}
