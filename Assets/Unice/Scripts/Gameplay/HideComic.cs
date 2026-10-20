using System;

namespace Unice
{
    /// <summary>What the Unice's player is shown while the others hide.</summary>
    public enum IntroMedia { None, Comic, Video }

    /// <summary>When the intro (video or comic) plays during the hiding phase and how much of the comic is on the page, with no UI in it.</summary>
    public static class HideComic
    {
        /// <summary>Where the intro videos live, under StreamingAssets.</summary>
        public const string VideoFolder = "Videos";

        /// <summary>
        /// During the opening (<paramref name="opening"/>) the story's video when it has one that has not failed to load (missing file,
        /// unsupported format), otherwise its comic, otherwise nothing. After the opening it is the comic only: everyone already saw the video.
        /// </summary>
        public static IntroMedia MediaFor(Story story, bool videoFailed, bool opening)
        {
            if (story == null) return IntroMedia.None;
            if (opening && story.hasIntroVideo && !videoFailed) return IntroMedia.Video;
            return story.hasComic ? IntroMedia.Comic : IntroMedia.None;
        }

        /// <summary>The video's path under StreamingAssets ("Videos/doll.mp4"), or null when the story has none.</summary>
        public static string VideoPath(Story story) => story != null && story.hasIntroVideo ? VideoFolder + "/" + story.introVideo : null;

        /// <summary>
        /// Everyone sees the opening together when the match starts (<see cref="Game.InIntro"/>). After that the Unice's player
        /// sees the comic while they wait with their eyes closed: always in the first round, and in later rounds only if they never saw it (<paramref name="seenBefore"/>).
        /// </summary>
        public static bool IsWanted(Game game, Character me, bool seenBefore)
        {
            if (game.phase != Phase.Hide || me == null) return false;
            var story = Stories.ForMap(game.mapId);
            if (story == null || !(story.hasComic || story.hasIntroVideo)) return false;
            if (game.InIntro) return true;
            return me.IsUnice && (game.round <= 1 || !seenBefore);
        }

        /// <summary>How many of the <paramref name="panelCount"/> panels are showing: the first at once, the rest spread evenly over the hiding time so the last one lands near its end.</summary>
        public static int PanelsShown(int panelCount, float hideTime, float timeLeft)
        {
            if (hideTime <= 0) return panelCount;
            float elapsed = Math.Max(0, Math.Min(hideTime, hideTime - timeLeft));
            return Math.Min(panelCount, 1 + (int)(elapsed / hideTime * panelCount));
        }
    }
}
