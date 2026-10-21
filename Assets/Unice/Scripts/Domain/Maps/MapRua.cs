namespace Unice
{
    /// <summary>
    /// Written by the map editor (Unice &gt; Editor de mapas) and still fine to edit by hand: the editor reads the compiled map (Maps.ById), not this file, so a hand edit shows up next time it opens.
    /// The street story (docs/stories/rua.md): the old square of the author's town, seen in a satellite print, where the cousins ran around
    /// playing hide-and-seek. Two streets run side by side (Rua do Mocambo and Rua Monsenhor José Magalhães) around a long island full of
    /// trees, join at one end and open into a wide paved square at the other. Going into the houses was forbidden, so every house is wall:
    /// the walkable ground is only the band of streets, island and square, made of overlapping areas with no doors.
    /// At the east end of the paved square stands the church, also off limits, with the cross in front of it and a phone booth beside.
    /// The print was turned on its side (its top is the west end) and stretched across to fit the 1720×1180 world.
    /// Room order matters: the game names the first area a point is in, so the most specific areas come first.
    /// Hiding spots (19), all of the furniture. Entry is the far corner of the paved square, in front of the church;
    /// spawn is the street in front of grandma's house.
    /// </summary>
    public static class MapRua
    {
        public const string Id = "rua";

        /// <summary>The church closing the paved square: not walkable, not see-through. Kept here to document the hole.</summary>
        public static readonly RectF Church = new RectF(1582, 300, 98, 600);

        public static MapDefinition Build() => new MapDefinition
        {
            id = Id,
            worldW = 1720,
            worldH = 1180,
            groundTint = "#6b6a63",
            entryX = 1480, entryY = 920,
            spawnX = 800, spawnY = 300,
            rooms = new[]
            {
                new Room("praca", 180, 300, 1060, 420, tint: "#5a6b3d"),
                new Room("largo", 1240, 210, 320, 780, tint: "#8a867a"),
                new Room("contorno", 100, 200, 200, 630),
                new Room("mocambo", 100, 200, 1240, 170),
                new Room("monsenhor", 100, 650, 1240, 180),
            },
            verticalDoors = new MapDoor[0],
            horizontalDoors = new MapDoor[0],
            furniture = new[]
            {
                new MapProp("gate", 700, 210, 120, 40, 's'),
                new MapProp("car", 500, 230, 190, 90, 's'),
                new MapProp("lamppost", 1000, 215, 36, 70, 's'),
                new MapProp("lamppost", 1180, 215, 36, 70, 's'),
                new MapProp("car", 760, 740, 190, 90, 'n'),
                new MapProp("bigtree", 210, 430, 150, 150, 's'),
                new MapProp("bigtree", 380, 320, 110, 110, 'e'),
                new MapProp("bigtree", 400, 520, 140, 140, 'n'),
                new MapProp("bigtree", 560, 610, 120, 100, 'n'),
                new MapProp("bigtree", 700, 380, 120, 120, 's'),
                new MapProp("bigtree", 900, 560, 130, 130, 'w'),
                new MapProp("bigtree", 1060, 360, 120, 120, 's'),
                new MapProp("bench", 600, 470, 110, 45, 's'),
                new MapProp("bench", 1080, 600, 110, 45, 'n'),
                new MapProp("popcorn", 1400, 300, 110, 70, 's'),
                new MapProp("cross", 1360, 560, 80, 180, 'w'),
                new MapProp("phonebooth", 1480, 700, 60, 90, 'w'),
                new MapProp("lamppost", 1130, 670, 36, 70, 'e'),
                new MapProp("bin", 1300, 920, 70, 70, 'n'),
            },
        };
    }
}
