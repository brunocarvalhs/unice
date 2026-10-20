namespace Unice
{
    /// <summary>
    /// Written by the map editor (Unice &gt; Editor de mapas) and still fine to edit by hand: the editor reads the compiled map (Maps.ById), not this file, so a hand edit shows up next time it opens.
    /// The werewolf story (docs/stories/lobisomem.md): the backyard of the aunt's house in the Bahia countryside, at night, drawn from
    /// the author's own floor plan. The house itself was off limits in the story, so it is a hole in the map (nobody can walk into it
    /// or see through it); the yard is the open ground around it, split into overlapping areas with no walls or doors.
    /// The only light is the balcony where the kids were listening, so it is the one warm floor in a very dark yard.
    /// The kids start on the balcony; the howl comes from the dark end of the aunt's garden, as far from them as the yard allows.
    /// Plan coordinates were scaled to the 1720×1180 world (the house map's size) for a comparable round length.
    /// Room order matters: the game names the first area a point is in, so the balcony comes first to win where areas overlap.
    /// Hiding spots (14) come first in the furniture list, the decorative clothesline (1) last.
    /// Entry is the dark end of the garden; spawn is the balcony, where the story was being told.
    /// </summary>
    public static class MapLobisomem
    {
        public const string Id = "lobisomem";

        /// <summary>The house: not walkable, not see-through. Areas are kept 22 px away from it (the "wall"). Kept here to document the hole.</summary>
        public static readonly RectF House = new RectF(557, 322, 648, 230);

        public static MapDefinition Build() => new MapDefinition
        {
            id = Id,
            worldW = 1720,
            worldH = 1180,
            groundTint = "#2a2018",
            entryX = 320, entryY = 1090,
            spawnX = 880, spawnY = 600,
            rooms = new[]
            {
                new Room("sacada", 557, 552, 648, 96, tint: "#b97a35"),
                new Room("fundos", 40, 40, 1640, 280),
                new Room("oeste", 40, 200, 495, 580),
                new Room("leste", 1227, 200, 453, 740),
                new Room("terreiro", 330, 588, 1350, 552),
                new Room("jardim", 40, 733, 390, 407, tint: "#223a26"),
            },
            verticalDoors = new MapDoor[0],
            horizontalDoors = new MapDoor[0],
            furniture = new[]
            {
                new MapProp("toolhouse", 40, 40, 323, 168, 'e'),
                new MapProp("coop", 40, 208, 248, 242, 'e'),
                new MapProp("coconut", 665, 49, 104, 92, 's'),
                new MapProp("bigtree", 174, 502, 196, 189, 'e'),
                new MapProp("coconut", 594, 955, 92, 92, 'n'),
                new MapProp("bush", 1034, 985, 196, 87, 'n'),
                new MapProp("watertank", 1443, 83, 220, 237, 's'),
                new MapProp("car", 1415, 687, 98, 172, 'w'),
                new MapProp("flowerbed", 80, 800, 150, 70, 's'),
                new MapProp("flowerbed", 250, 950, 120, 70, 'e'),
                new MapProp("flowerbed", 70, 1040, 140, 60, 'n'),
                new MapProp("woodpile", 760, 880, 110, 80, 'n'),
                new MapProp("barrel", 1180, 760, 60, 60, 'w'),
                new MapProp("doghouse", 1300, 1000, 100, 80, 'w'),
                new MapProp("clothesline", 780, 700, 220, 20),
            },
        };
    }
}
