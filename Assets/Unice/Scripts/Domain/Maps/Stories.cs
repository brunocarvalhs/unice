using System.Linq;

namespace Unice
{
    /// <summary>What a comic figure is drawn with.</summary>
    public enum ComicFigureKind
    {
        /// <summary>A picture from Resources/Art (furniture, the doll…), named by <see cref="ComicFigure.art"/>.</summary>
        Sprite,
        /// <summary>One of the story's cousins, drawn with the game's layered kid sprites (<see cref="ComicFigure.cousin"/>).</summary>
        Kid,
        /// <summary>Aunt Unice, standing.</summary>
        Unice,
    }

    /// <summary>One thing standing in a comic panel. Positions are fractions of the panel (x left→right, y top→bottom, y being the feet line).</summary>
    public sealed class ComicFigure
    {
        public readonly ComicFigureKind kind;
        /// <summary>Sprite file in Resources/Art without extension ("furn_20", "doll"); only for <see cref="ComicFigureKind.Sprite"/>.</summary>
        public readonly string art;
        /// <summary>Which cousin look to use (0..4); only for <see cref="ComicFigureKind.Kid"/>. Wraps around if larger.</summary>
        public readonly int cousin;
        public readonly float x, y;
        /// <summary>Sprite width in panel pixels (kids and the Unice have their own size).</summary>
        public readonly float width;
        /// <summary>Mirrored (facing left).</summary>
        public readonly bool flip;
        /// <summary>Bobs up and down a little, so the panel feels alive.</summary>
        public readonly bool bob;

        ComicFigure(ComicFigureKind kind, string art, int cousin, float x, float y, float width, bool flip, bool bob)
        { this.kind = kind; this.art = art; this.cousin = cousin; this.x = x; this.y = y; this.width = width; this.flip = flip; this.bob = bob; }

        public static ComicFigure Sprite(string art, float width, float x, float y, bool bob = false) => new ComicFigure(ComicFigureKind.Sprite, art, 0, x, y, width, false, bob);
        public static ComicFigure Kid(int cousin, float x, float y, bool flip = false) => new ComicFigure(ComicFigureKind.Kid, null, cousin, x, y, 0, flip, true);
        public static ComicFigure Unice(float x, float y, bool flip = false) => new ComicFigure(ComicFigureKind.Unice, null, 0, x, y, 0, flip, false);
    }

    /// <summary>One panel of the intro comic: a backdrop and who stands in front of it (later figures are drawn over earlier ones). The caption is "comic.{story}.{N}" in Localization.</summary>
    public sealed class ComicPanel
    {
        /// <summary>Floor picture in Resources/Art ("floor_kitchen"), or null for the map's flat ground color.</summary>
        public readonly string backdrop;
        public readonly ComicFigure[] figures;

        public ComicPanel(string backdrop, params ComicFigure[] figures) { this.backdrop = backdrop; this.figures = figures; }
    }

    /// <summary>
    /// One story the match can be set in: it picks the map and may bring an intro comic for the Unice while the others hide.
    /// Display text (name, blurb, comic captions) lives in Localization, keyed by <see cref="id"/>.
    /// </summary>
    public sealed class Story
    {
        /// <summary>Short key: "stories.{id}.name" / "stories.{id}.blurb" / "comic.{id}.N" in Localization.</summary>
        public readonly string id;
        public readonly string mapId;
        /// <summary>False while the map is a work in progress: the picker shows it as "coming soon" (still playable in dev builds).</summary>
        public readonly bool released;
        /// <summary>The intro comic's panels, in reading order (empty = no comic).</summary>
        public readonly ComicPanel[] comic;
        /// <summary>True when the hiding phase tells the story as a comic.</summary>
        public readonly bool hasComic;
        /// <summary>Picture on the story's card in Resources/Art, or null for a plain card.</summary>
        public readonly string cardArt;

        public Story(string id, string mapId, bool released, ComicPanel[] comic = null, string cardArt = null)
        {
            this.id = id; this.mapId = mapId; this.released = released; this.cardArt = cardArt;
            this.comic = comic ?? new ComicPanel[0]; hasComic = this.comic.Length > 0;
        }
    }

    /// <summary>Registry of the stories, in the order the picker shows them. To add one, follow docs/stories/README.md.</summary>
    public static class Stories
    {
        public const string DollId = "doll";

        public static readonly Story[] All =
        {
            new Story(DollId, MapHouse.Id, released: true, cardArt: "floor_master", comic: new[]
            {
                // the kitchen: kids running around grandma's table
                new ComicPanel("floor_kitchen",
                    ComicFigure.Sprite("furn_20", 80, .22f, .62f), ComicFigure.Sprite("furn_21", 100, .74f, .6f),
                    ComicFigure.Kid(0, .4f, .88f), ComicFigure.Kid(1, .55f, .8f), ComicFigure.Kid(2, .7f, .9f, flip: true)),
                // the aunt's room: the Unice watching the doll, Márcio peeking in
                new ComicPanel("floor_master",
                    ComicFigure.Sprite("furn_1", 60, .14f, .66f), ComicFigure.Sprite("furn_0", 100, .78f, .68f),
                    ComicFigure.Sprite("doll", 26, .78f, .5f, bob: true), ComicFigure.Unice(.5f, .86f), ComicFigure.Kid(0, .22f, .92f)),
                // the scrap: the doll passes from hand to hand
                new ComicPanel("floor_living",
                    ComicFigure.Sprite("furn_14", 120, .5f, .56f),
                    ComicFigure.Kid(1, .22f, .88f), ComicFigure.Kid(2, .5f, .84f), ComicFigure.Kid(3, .78f, .88f, flip: true),
                    ComicFigure.Sprite("doll", 24, .36f, .42f, bob: true)),
                // everyone in a corner: hiders peek out from behind the furniture
                new ComicPanel("floor_dining",
                    ComicFigure.Kid(2, .22f, .8f), ComicFigure.Kid(4, .74f, .84f),
                    ComicFigure.Sprite("furn_1", 64, .2f, .78f), ComicFigure.Sprite("furn_17", 130, .74f, .82f), ComicFigure.Unice(.5f, .9f, flip: true)),
                // your turn: the Unice with her eyes closed and a doll to find
                new ComicPanel("floor_dining",
                    ComicFigure.Unice(.5f, .9f), ComicFigure.Sprite("doll", 30, .78f, .6f, bob: true), ComicFigure.Sprite("furn_17", 110, .22f, .8f)),
            }),
        };

        /// <summary>The story told on <paramref name="mapId"/>, or null for an unknown map.</summary>
        public static Story ForMap(string mapId) => All.FirstOrDefault(story => story.mapId == mapId);
    }
}
