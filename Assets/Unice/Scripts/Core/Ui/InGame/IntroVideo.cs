using UnityEngine;
using UnityEngine.Video;

namespace Unice
{
    /// <summary>
    /// One explainer video from StreamingAssets, played by Unity's VideoPlayer onto a texture the UI can show.
    /// It never throws: a missing file, an unsupported format or a stuck prepare ends as <see cref="State.Failed"/> and the caller falls back to the comic.
    /// </summary>
    sealed class IntroVideo
    {
        public enum State { Idle, Preparing, Playing, Finished, Failed }

        /// <summary>A video that has not prepared by now is given up on (a missing file may never report an error).</summary>
        const float PrepareTimeoutSeconds = 6;

        public State Current { get; private set; } = State.Idle;
        /// <summary>The frames, once <see cref="Current"/> is <see cref="State.Playing"/> or later.</summary>
        public RenderTexture Texture { get; private set; }

        GameObject host;
        VideoPlayer player;
        float preparingFor;

        /// <summary>Starts loading <paramref name="pathUnderStreamingAssets"/> ("Videos/doll.mp4"); it plays by itself once ready.</summary>
        public void Play(string pathUnderStreamingAssets, bool muted)
        {
            Stop();
            if (player == null)
            {
                host = new GameObject("IntroVideo");
                player = host.AddComponent<VideoPlayer>();
                player.playOnAwake = false; player.isLooping = false; player.skipOnDrop = true;
                player.renderMode = VideoRenderMode.RenderTexture;
                player.audioOutputMode = VideoAudioOutputMode.Direct;
                player.prepareCompleted += OnPrepared;
                player.errorReceived += (_, message) => { Debug.LogWarning("Intro video: " + message); Current = State.Failed; };
                player.loopPointReached += _ => Current = State.Finished;
            }
            player.url = Application.streamingAssetsPath + "/" + pathUnderStreamingAssets;
            player.SetDirectAudioMute(0, muted);
            Current = State.Preparing; preparingFor = 0;
            player.Prepare();
        }

        /// <summary>Per frame while the video is wanted: gives up on a prepare that never finishes.</summary>
        public void Tick(float dt)
        {
            if (Current != State.Preparing) return;
            preparingFor += dt;
            if (preparingFor > PrepareTimeoutSeconds) Current = State.Failed;
        }

        /// <summary>Stops the video and lets go of its frames.</summary>
        public void Stop()
        {
            if (player != null && (player.isPlaying || player.isPrepared)) player.Stop();
            if (Texture != null) { player.targetTexture = null; Texture.Release(); Object.Destroy(Texture); Texture = null; }
            Current = State.Idle;
        }

        void OnPrepared(VideoPlayer source)
        {
            if (Current != State.Preparing) return;   // stopped while it was loading
            Texture = new RenderTexture((int)source.width, (int)source.height, 0);
            source.targetTexture = Texture;
            source.Play();
            Current = State.Playing;
        }
    }
}
