using System.Collections.Generic;

namespace Unice
{
    /// <summary>The mixing rules of the sound effects, with no audio in them: how loud each sound is and which voice plays it.</summary>
    public static class SfxMix
    {
        /// <summary>The voice only <see cref="IsCritical"/> sounds use.</summary>
        public const int ReservedVoice = 0;

        /// <summary>
        /// Volume multiplier per sound, so the mix is even: the synthesized heart beat is ~3x louder than the giggle, and
        /// footsteps repeat all the time. Sounds not listed play at 1. Tune by ear on a phone speaker and on headphones.
        /// </summary>
        static readonly Dictionary<string, float> Gains = new Dictionary<string, float> { ["heart"] = .35f, ["step"] = .6f };

        /// <summary>The sounds that tell the player what is happening: the giggle, a catch, the Unice coming in.</summary>
        public static bool IsCritical(string name) => name == "giggle" || name == "sting" || name == "boom";

        public static float GainFor(string name) => Gains.TryGetValue(name, out float gain) ? gain : 1;

        /// <summary>The volume a sound plays at: the requested volume times its gain, kept in 0..1.</summary>
        public static float VolumeFor(string name, float volume) => Geometry.Clamp(volume * GainFor(name), 0, 1);

        /// <summary>
        /// The voice a sound plays on: critical sounds always get the reserved voice; the rest take turns on the others
        /// (<paramref name="nextVoice"/> advances, skipping the reserved one).
        /// </summary>
        public static int PickVoice(bool critical, ref int nextVoice, int voiceCount)
        {
            if (critical) return ReservedVoice;
            int voice = nextVoice;
            nextVoice = voice + 1 < voiceCount ? voice + 1 : ReservedVoice + 1;
            return voice;
        }
    }
}
