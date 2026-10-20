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

        /// <summary>The story's video when it has one that has not failed to load (missing file, unsupported format), otherwise its comic, otherwise nothing.</summary>
        public static IntroMedia MediaFor(Story story, bool videoFailed)
        {
            if (story == null) return IntroMedia.None;
            if (story.hasIntroVideo && !videoFailed) return IntroMedia.Video;
            return story.hasComic ? IntroMedia.Comic : IntroMedia.None;
        }

        /// <summary>The video's path under StreamingAssets ("Videos/doll.mp4"), or null when the story has none.</summary>
        public static string VideoPath(Story story) => story != null && story.hasIntroVideo ? VideoFolder + "/" + story.introVideo : null;

        /// <summary>
        /// The Unice's player sees the intro while they wait with their eyes closed: always in the first round,
        /// and in later rounds only if they never saw it (<paramref name="seenBefore"/>).
        /// </summary>
        public static bool IsWanted(Game game, Character me, bool seenBefore)
        {
            if (game.phase != Phase.Hide || me == null || !me.IsUnice) return false;
            var story = Stories.ForMap(game.mapId);
            return story != null && (story.hasComic || story.hasIntroVideo) && (game.round <= 1 || !seenBefore);
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
