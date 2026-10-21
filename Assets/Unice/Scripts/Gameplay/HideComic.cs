namespace Unice
{
    /// <summary>What the Unice's player is shown while the others hide.</summary>
    public enum IntroMedia { None, Video }

    /// <summary>When the story's opening video plays during the hiding phase, with no UI in it.</summary>
    public static class HideComic
    {
        /// <summary>Where the intro videos live, under StreamingAssets.</summary>
        public const string VideoFolder = "Videos";

        /// <summary>During the opening (<paramref name="opening"/>) the story's video when it has one that has not failed to load (missing file, unsupported format), otherwise nothing.</summary>
        public static IntroMedia MediaFor(Story story, bool videoFailed, bool opening)
        {
            if (story == null) return IntroMedia.None;
            return opening && story.hasIntroVideo && !videoFailed ? IntroMedia.Video : IntroMedia.None;
        }

        /// <summary>The video's path under StreamingAssets ("Videos/doll.mp4"), or null when the story has none.</summary>
        public static string VideoPath(Story story) => story != null && story.hasIntroVideo ? VideoFolder + "/" + story.introVideo : null;

        /// <summary>Everyone sees the opening video together when the first match starts (<see cref="Game.InIntro"/>); there is nothing to show outside that window.</summary>
        public static bool IsWanted(Game game, Character me)
        {
            if (game.phase != Phase.Hide || me == null) return false;
            var story = Stories.ForMap(game.mapId);
            return story != null && story.hasIntroVideo && game.InIntro;
        }
    }
}
