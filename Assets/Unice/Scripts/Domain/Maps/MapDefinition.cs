using System.Collections.Generic;
using System.Linq;

namespace Unice
{
    /// <summary>
    /// Data for one playable map (the house, …): what <see cref="Map.Load"/> turns into rooms, doors,
    /// furniture, hiding spots and the navigation graph. Display text (room names, hiding-spot phrases, the map's
    /// own name) is never stored here — it lives in Core's Localization table, keyed by id/type.
    /// </summary>
    public sealed class MapDefinition
    {
        public string id;
        public float worldW, worldH;
        public Room[] rooms;
        /// <summary>Openings in horizontal walls (walked through vertically) and in vertical walls.</summary>
        public MapDoor[] verticalDoors, horizontalDoors;
        /// <summary>Obstacles and hiding spots, in one list (the art manifest refers to them by position in it).</summary>
        public MapProp[] furniture;
        /// <summary>Where the Unice walks in from (<see cref="Map.Entrance"/>).</summary>
        public float entryX, entryY;
        /// <summary>Where hiders line up at the start of a round.</summary>
        public float spawnX, spawnY;
        /// <summary>Flat floor color used where there is no painted floor art.</summary>
        public string groundTint;
    }

    /// <summary>One opening between rooms: the center of the wall gap it crosses.</summary>
    public readonly struct MapDoor
    {
        public readonly float x, y;
        public MapDoor(float x, float y) { this.x = x; this.y = y; }
    }

    /// <summary>
    /// One prop: a decorative obstacle, or a hiding spot when <see cref="hideSide"/> is set (non-'\0').
    /// When hideable, its phrase ("under the bed") lives in Localization as "spot." + type.
    /// </summary>
    public sealed class MapProp
    {
        /// <summary>Art key (e.g. "hedge", "shed"), also the "spot." localization key when hideable.</summary>
        public string type;
        public float x, y, w, h;
        /// <summary>'\0' = decorative only; otherwise the side (n/s/e/w) a child approaches it from to hide.</summary>
        public char hideSide;
        public MapProp(string type, float x, float y, float w, float h, char hideSide = '\0')
        { this.type = type; this.x = x; this.y = y; this.w = w; this.h = h; this.hideSide = hideSide; }
    }

    /// <summary>Registry of every playable map, by id. To add a map, add its <c>Build()</c> to <see cref="InOrder"/> (the one place).</summary>
    public static class Maps
    {
        /// <summary>Every map, in the order the waiting room's map stepper cycles through them.</summary>
        static readonly MapDefinition[] InOrder = { MapHouse.Build() };

        public static readonly Dictionary<string, MapDefinition> All = InOrder.ToDictionary(definition => definition.id);
        public static readonly string[] AllIds = InOrder.Select(definition => definition.id).ToArray();
        public static MapDefinition ById(string id) => All.TryGetValue(id, out var definition) ? definition : All[MapHouse.Id];
    }
}
