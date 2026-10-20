using UnityEngine;

namespace Unice
{
    /// <summary>Which loop should be heard right now.</summary>
    public enum MusicTrack { None, Menu, Match }

    /// <summary>
    /// Plays the menu theme (the forró track in Resources/Audio, or <see cref="MenuMusic"/> synthesized if the file cannot be loaded)
    /// and the match soundtrack (<see cref="MatchMusic"/>) on loops, fading one out as the other fades in.
    /// </summary>
    public sealed class Music
    {
        /// <summary>Loudness of the loops (they sit under the sound effects) and how fast they fade (volume per second).</summary>
        const float MatchVolume = .3f, MenuVolume = .25f, FadeRate = 1.2f;

        /// <summary>The menu theme's file under Resources (no extension).</summary>
        const string MenuClipPath = "Audio/menu_theme";

        readonly AudioSource menu, match;

        public Music(Transform parent)
        {
            var menuClip = Resources.Load<AudioClip>(MenuClipPath);
            if (menuClip == null) menuClip = ClipSynth.Synthesize("music_menu", MenuMusic.Loop());
            menu = CreateSource(parent, "music_menu", menuClip);
            match = CreateSource(parent, "music_match", ClipSynth.Synthesize("music_match", MatchMusic.Loop()));
        }

        static AudioSource CreateSource(Transform parent, string name, AudioClip clip)
        {
            var musicObject = new GameObject(name); musicObject.transform.SetParent(parent, false);
            var source = musicObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = true; source.spatialBlend = 0; source.volume = 0;
            source.clip = clip;
            return source;
        }

        /// <summary>Per step.</summary>
        public void Tick(float dt, MusicTrack wanted)
        {
            Fade(menu, dt, wanted == MusicTrack.Menu ? MenuVolume : 0);
            Fade(match, dt, wanted == MusicTrack.Match ? MatchVolume : 0);
        }

        static void Fade(AudioSource source, float dt, float target)
        {
            source.volume = Mathf.MoveTowards(source.volume, target, FadeRate * dt);
            if (target > 0 && !source.isPlaying) source.Play();
            else if (target <= 0 && source.volume <= 0 && source.isPlaying) source.Pause();
        }
    }
}
