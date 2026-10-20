using System;

namespace Unice
{
    /// <summary>
    /// Builds the game's sounds from simple tones, like the web version's WebAudio code: each tone is a wave whose pitch
    /// slides from <see cref="Tone.frequency"/> to <see cref="Tone.endFrequency"/> and whose volume decays to silence.
    /// This renders the samples; turning them into an audio clip is the engine layer's job.
    /// </summary>
    public static class ToneSynth
    {
        public const int SampleRate = 44100;
        /// <summary>Overall loudness boost applied to every sample before clipping.</summary>
        const float MasterGain = 3.2f;
        /// <summary>A little silence after the last tone.</summary>
        const float Tail = .03f;
        /// <summary>Where a tone's volume ends up: (almost) silence.</summary>
        const float EndVolume = .0001f;

        public enum Wave { Square, Triangle, Saw }

        public struct Tone
        {
            public Wave wave;
            /// <summary>Start and end pitch, in Hz (endFrequency 0 = no slide).</summary>
            public float frequency, endFrequency;
            public float duration, volume;
            /// <summary>Seconds after the start of the sound.</summary>
            public float delay;
        }

        public static Tone MakeTone(float frequency, float duration, Wave wave, float volume, float endFrequency = 0, float delay = 0) =>
            new Tone { frequency = frequency, duration = duration, wave = wave, volume = volume, endFrequency = endFrequency, delay = delay };

        /// <summary>A run of notes, one every <paramref name="gap"/> seconds; <paramref name="pitchBend"/> ≠ 1 slides each note by that factor.</summary>
        public static Tone[] ToneSequence(float[] frequencies, float duration, Wave wave, float volume, float gap, float pitchBend)
        {
            var tones = new Tone[frequencies.Length];
            for (int i = 0; i < frequencies.Length; i++)
                tones[i] = MakeTone(frequencies[i], duration, wave, volume, pitchBend != 1 ? frequencies[i] * pitchBend : 0, i * gap);
            return tones;
        }

        /// <summary>How many samples the tones take, with the tail of silence.</summary>
        public static int SampleCount(Tone[] tones)
        {
            float length = 0;
            foreach (var tone in tones) length = Math.Max(length, tone.delay + tone.duration + Tail);
            return (int)Math.Ceiling(length * SampleRate);
        }

        /// <summary>Mixes the tones into mono samples (-1..1).</summary>
        public static float[] Render(Tone[] tones)
        {
            int sampleCount = SampleCount(tones);
            var samples = new float[sampleCount];
            foreach (var tone in tones)
            {
                int start = (int)(tone.delay * SampleRate), toneSamples = (int)(tone.duration * SampleRate);
                double phase = 0;
                for (int i = 0; i < toneSamples && start + i < sampleCount; i++)
                {
                    float progress = (float)i / toneSamples;
                    float frequency = tone.endFrequency > 0 ? tone.frequency * MathF.Pow(tone.endFrequency / tone.frequency, progress) : tone.frequency;
                    float gain = tone.volume * MathF.Pow(EndVolume / tone.volume, progress);   // exponential decay to (almost) silence
                    phase += frequency / SampleRate;
                    float cycle = (float)(phase - Math.Floor(phase));   // 0..1 within the current period
                    samples[start + i] += WaveAt(tone.wave, cycle) * gain * MasterGain;
                }
            }
            for (int i = 0; i < sampleCount; i++) samples[i] = Geometry.Clamp(samples[i], -1, 1);
            return samples;
        }

        /// <summary>The wave's value (-1..1) at a point (0..1) of its period.</summary>
        public static float WaveAt(Wave wave, float cycle) => wave switch
        {
            Wave.Square => cycle < .5f ? 1 : -1,
            Wave.Triangle => cycle < .5f ? 4 * cycle - 1 : 3 - 4 * cycle,
            _ => 2 * cycle - 1,   // saw
        };
    }
}
