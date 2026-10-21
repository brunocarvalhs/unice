using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The story's opening video: when the match starts every player watches it together while the match waits
    /// (<see cref="HideComic"/> decides when). Tap or click skips it — the host (or a solo player) ends it for everyone,
    /// a guest only closes their own copy and waits for the host's clock behind it.
    /// </summary>
    sealed class HideComicView : UiPart
    {
        public readonly VisualElement Root;
        readonly Label countdown;
        readonly VisualElement videoSurface, skipHint;
        readonly IntroVideo video = new IntroVideo();
        /// <summary>The video could not be played (missing file, bad format): nothing fills the wait for the rest of the session.</summary>
        bool videoFailed;
        IntroVideo.State lastVideoState;
        bool skipped;

        /// <summary>True while the video covers the screen (the black "heading home" screen stays hidden then).</summary>
        public bool Playing { get; private set; }

        public HideComicView(Ui ui) : base(ui)
        {
            Root = AddElement(ui.safeArea, "comic");
            var header = IgnorePointer(AddElement(Root, "comic-head"));
            countdown = IgnorePointer(AddLabel(header, "", "comic-count"));
            videoSurface = IgnorePointer(AddElement(Root, "comic-video"));
            skipHint = IgnorePointer(LocalizedLabel(Root, "intro.skip", "comic-skip"));
            Root.RegisterCallback<PointerDownEvent>(_ => Skip());
            Show(Root, false);
        }

        /// <summary>Per frame: starts, advances and ends the opening video according to the game state.</summary>
        public void Frame(float dt)
        {
            if (game.phase != Phase.Hide) skipped = false;

            bool wanted = !skipped && HideComic.IsWanted(game, app.myCharacter);
            if (wanted != Playing)
            {
                Playing = wanted; Show(Root, wanted);
                if (wanted) Begin();
            }
            if (!Playing) { StopVideo(); return; }

            UpdateVideo(dt);
            Show(skipHint, true);
            if (WantsSkipKey()) Skip();
            SetText(countdown, Localization.Get("intro.countdown", Mathf.CeilToInt(game.IntroTimeLeft)));
        }

        /// <summary>
        /// Skips the opening. It is everyone's wait, so only the host (or a solo player) ends it for all, by dropping its extra seconds;
        /// a guest only closes their own video and waits for the host's clock behind it.
        /// </summary>
        void Skip()
        {
            if (game.InIntro && game.isHost) game.timeLeft = game.hideTime;
            else skipped = true;
        }

        /// <summary>Enter, Space or the gamepad's A skip too.</summary>
        static bool WantsSkipKey()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current; var pad = UnityEngine.InputSystem.Gamepad.current;
            return (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
                || (pad != null && (pad.buttonSouth.wasPressedThisFrame));
        }

        /// <summary>The opening starts: hides any stale frame from a previous session, then starts the video.</summary>
        void Begin()
        {
            Show(videoSurface, false);
            var story = Stories.ForMap(game.mapId);
            if (story == null) return;
            StartVideo(story);
        }

        /// <summary>The opening video starts loading, if the story has one and it has not failed before.</summary>
        void StartVideo(Story story)
        {
            lastVideoState = IntroVideo.State.Idle;
            if (HideComic.MediaFor(story, videoFailed, opening: game.InIntro) != IntroMedia.Video) { ShowVideo(false); return; }
            video.Play(HideComic.VideoPath(story), muted: !app.sfx.Enabled);
        }

        /// <summary>Per frame: advances the video and reacts to it starting, failing or finishing.</summary>
        void UpdateVideo(float dt)
        {
            video.Tick(dt);
            var state = video.Current;
            if (state == lastVideoState) return;
            lastVideoState = state;
            string storyId = Stories.ForMap(game.mapId)?.id;
            switch (state)
            {
                case IntroVideo.State.Playing:
                    videoSurface.style.backgroundImage = Background.FromRenderTexture(video.Texture);
                    ShowVideo(true);
                    Metrics.Log("intro_video", ("story", storyId), ("result", "play"));
                    break;
                case IntroVideo.State.Failed:
                    videoFailed = true; video.Stop(); ShowVideo(false);
                    Metrics.Log("intro_video", ("story", storyId), ("result", "failed"));
                    break;
                case IntroVideo.State.Finished:
                    break;   // the last frame stays up until the opening's wait is over
            }
        }

        void StopVideo()
        {
            if (video.Current != IntroVideo.State.Idle) video.Stop();
            ShowVideo(false);
        }

        void ShowVideo(bool visible) => Show(videoSurface, visible);
    }
}
