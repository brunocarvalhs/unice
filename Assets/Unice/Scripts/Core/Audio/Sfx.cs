using System.Collections.Generic;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// The game's sound effects on the speakers: the synthesized sounds of <see cref="SfxCatalog"/> played with a volume, a
    /// left/right pan, and a muffled filter when there is a wall in between. The mixing rules (which voice, how loud) are
    /// <see cref="SfxMix"/>.
    /// </summary>
    public sealed class Sfx
    {
        /// <summary>How many sounds can play at once; the oldest is cut when a new one starts.</summary>
        const int Voices = 12;
        /// <summary>PlayerPrefs key of the sound on/off setting (1 = on).</summary>
        const string SoundKey = "unice.sound";
        /// <summary>Low-pass cutoff (Hz) for sounds heard through a wall.</summary>
        const float MuffledCutoff = 700;

        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly List<(AudioSource source, AudioLowPassFilter muffle)> voices = new List<(AudioSource, AudioLowPassFilter)>();
        int nextVoice = SfxMix.ReservedVoice + 1;
        bool enabled = PlayerPrefs.GetInt(SoundKey, 1) != 0;

        /// <summary>The player's sound on/off setting, saved on the device.</summary>
        public bool Enabled
        {
            get => enabled;
            set { enabled = value; PlayerPrefs.SetInt(SoundKey, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public Sfx(Transform parent)
        {
            for (int i = 0; i < Voices; i++)
            {
                var voiceObject = new GameObject("sfx" + i); voiceObject.transform.SetParent(parent, false);
                var source = voiceObject.AddComponent<AudioSource>(); source.playOnAwake = false; source.spatialBlend = 0;
                var muffle = voiceObject.AddComponent<AudioLowPassFilter>(); muffle.cutoffFrequency = MuffledCutoff; muffle.enabled = false;
                voices.Add((source, muffle));
            }
            foreach (var sound in SfxCatalog.Sounds) clips[sound.Key] = ClipSynth.Synthesize(sound.Key, sound.Value);
        }

        /// <param name="volume">0..1.</param>
        /// <param name="pan">-1 (left) .. 1 (right).</param>
        /// <param name="muffled">Heard through a wall.</param>
        public void Play(string name, float volume = 1, float pan = 0, bool muffled = false)
        {
            if (!Enabled || volume <= 0 || !clips.TryGetValue(name, out var clip)) return;
            var (source, muffle) = voices[SfxMix.PickVoice(SfxMix.IsCritical(name), ref nextVoice, voices.Count)];
            source.Stop(); source.clip = clip;
            source.volume = SfxMix.VolumeFor(name, volume); source.panStereo = Mathf.Clamp(pan, -1, 1); muffle.enabled = muffled;
            source.Play();
        }
    }
}
