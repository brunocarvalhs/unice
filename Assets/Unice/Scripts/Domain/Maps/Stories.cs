using System.Linq;

namespace Unice
{
    /// <summary>
    /// One story the match can be set in: it picks the map and may bring an opening video every player watches when the match starts.
    /// Display text (name, blurb) lives in Localization, keyed by <see cref="id"/>.
    /// </summary>
    public sealed class Story
    {
        /// <summary>Short key: "stories.{id}.name" / "stories.{id}.blurb" in Localization.</summary>
        public readonly string id;
        public readonly string mapId;
        /// <summary>False while the map is a work in progress: the picker shows it as "coming soon" (still playable in dev builds).</summary>
        public readonly bool released;
        /// <summary>Picture on the story's card in Resources/Art, or null for a plain card.</summary>
        public readonly string cardArt;
        /// <summary>File name of the opening video every player watches when the match starts, in StreamingAssets/Videos (null = none).</summary>
        public readonly string introVideo;
        /// <summary>Seconds the match waits in the first round while the opening video plays (a little longer than the video itself); 0 = no wait.</summary>
        public readonly float introSeconds;
        /// <summary>True when the story brings an intro video.</summary>
        public readonly bool hasIntroVideo;

        public Story(string id, string mapId, bool released, string cardArt = null, string introVideo = null, float introSeconds = 0)
        {
            this.id = id; this.mapId = mapId; this.released = released; this.cardArt = cardArt;
            this.introVideo = introVideo; hasIntroVideo = !string.IsNullOrEmpty(introVideo); this.introSeconds = introSeconds;
        }
    }

    /// <summary>Registry of the stories, in the order the picker shows them. To add one, follow docs/stories/README.md.</summary>
    public static class Stories
    {
        public const string DollId = "doll";
        /// <summary>The doll video lasts 29.1 s; the match waits this long (a bit more, so the last frame is never cut) in round 1.</summary>
        const float DollIntroSeconds = 29.5f;

        public static readonly Story[] All =
        {
            new Story(DollId, MapHouse.Id, released: true, cardArt: "floor_master", introVideo: "doll.mp4", introSeconds: DollIntroSeconds),
            new Story("lobisomem", MapLobisomem.Id, released: false),
            new Story("rua", MapRua.Id, released: false),
        };

        /// <summary>The story told on <paramref name="mapId"/>, or null for an unknown map.</summary>
        public static Story ForMap(string mapId) => All.FirstOrDefault(story => story.mapId == mapId);
    }
}
