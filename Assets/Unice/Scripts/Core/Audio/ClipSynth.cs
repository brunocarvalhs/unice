using UnityEngine;

namespace Unice
{
    /// <summary>Turns the samples rendered by <see cref="ToneSynth"/> into an Unity audio clip.</summary>
    static class ClipSynth
    {
        public static AudioClip Synthesize(string name, ToneSynth.Tone[] tones)
        {
            var samples = ToneSynth.Render(tones);
            var clip = AudioClip.Create(name, samples.Length, 1, ToneSynth.SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
