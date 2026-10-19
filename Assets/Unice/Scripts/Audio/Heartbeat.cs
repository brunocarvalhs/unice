using System;

namespace Unice
{
    /// <summary>A hider hears (and sees, as a red vignette) their heart beat faster the closer the Unice gets.</summary>
    public sealed class Heartbeat
    {
        /// <summary>Seconds between beats when the Unice is right here, and how much longer it gets at the edge of the range.</summary>
        const float FastestBeat = .42f, BeatSlowdown = .7f;
        /// <summary>How fast the pulse fades (per second).</summary>
        const float PulseFadeRate = 2.5f;

        readonly Game game;
        readonly Func<Character> myCharacter;
        readonly IStage stage;
        float beatTimer;

        /// <summary>0 (far or not hunting) .. 1 (the Unice is right here).</summary>
        public float Intensity { get; private set; }
        /// <summary>1 on each beat, fading to 0; drives the vignette's throb.</summary>
        public float Pulse { get; private set; }

        public Heartbeat(Game game, Func<Character> myCharacter, IStage stage) { this.game = game; this.myCharacter = myCharacter; this.stage = stage; }

        public void Stop() => Intensity = 0;

        /// <summary>Seconds until the next beat: faster the closer the Unice is (intensity 0..1).</summary>
        public static float BeatInterval(float intensity) => FastestBeat + (1 - intensity) * BeatSlowdown;

        /// <summary>Per step while a round is live and not paused.</summary>
        public void Tick(float dt)
        {
            var me = myCharacter(); var uniceCharacter = game.unice;
            bool canFeelIt = game.phase == Phase.Hunt && me != null && !me.IsUnice && !me.outOfRound && uniceCharacter != null && uniceCharacter.x > 0;
            if (!canFeelIt) { Intensity = 0; return; }
            float distance = Geometry.Distance(me, uniceCharacter);
            Intensity = distance < Rules.HeartbeatRange ? 1 - distance / Rules.HeartbeatRange : 0;
            beatTimer -= dt;
            if (Intensity > 0 && beatTimer <= 0)
            {
                beatTimer = BeatInterval(Intensity);
                Pulse = 1;
                stage.Sound("heart");
            }
        }

        /// <summary>Per frame: the pulse fades out.</summary>
        public void Fade(float dt) => Pulse = Math.Max(0, Pulse - dt * PulseFadeRate);
    }
}
