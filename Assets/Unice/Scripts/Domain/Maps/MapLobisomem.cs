namespace Unice
{
    /// <summary>
    /// The werewolf story (docs/stories/lobisomem.md): a farmhouse in the countryside at night — the house on top, a porch
    /// across the middle and the backyard below. The kids were locked out in the yard, so they start there (spawn, left);
    /// the howl comes from the far back of the corral (entry, bottom right), as far from them as the map allows.
    /// Same world scale as the house (1720×1180) for a comparable round length.
    /// </summary>
    public static class MapLobisomem
    {
        public const string Id = "lobisomem";

        public static MapDefinition Build() => new MapDefinition
        {
            id = Id,
            worldW = 1720,
            worldH = 1180,
            groundTint = "#2f4a3a",   // dark earth and grass, by night
            entryX = 1620, entryY = 1090,
            spawnX = 300, spawnY = 740,
            rooms = new[]
            {
                new Room("sala", 40, 40, 520, 340),
                new Room("cozinha", 584, 40, 480, 340),
                new Room("quarto", 1088, 40, 592, 340),
                new Room("varanda", 40, 404, 1640, 120),
                new Room("terreiro", 40, 548, 520, 592),
                new Room("pomar", 584, 548, 480, 592),
                new Room("curral", 1088, 548, 592, 592),
            },
            verticalDoors = new[]
            {
                new MapDoor(440, 380), new MapDoor(824, 380), new MapDoor(1384, 380),   // house -> porch
                new MapDoor(440, 524), new MapDoor(824, 524), new MapDoor(1384, 524),   // porch -> yard
            },
            horizontalDoors = new[]
            {
                new MapDoor(560, 210), new MapDoor(1064, 210),   // sala|cozinha, cozinha|quarto
                new MapDoor(560, 850), new MapDoor(1064, 850),   // terreiro|pomar, pomar|curral
            },
            furniture = new[]
            {
                // --- hiding spots (23) ---
                new MapProp("sofa", 80, 300, 200, 56, 'n'),
                new MapProp("armchair", 80, 80, 60, 60, 's'),
                new MapProp("hammock", 300, 100, 140, 60, 's'),
                new MapProp("ktable", 720, 200, 130, 80, 's'),
                new MapProp("sinkcab", 640, 50, 150, 44, 's'),
                new MapProp("bed", 1130, 60, 150, 175, 's'),
                new MapProp("wardrobe", 1100, 290, 58, 90, 'e'),
                new MapProp("chest", 1450, 60, 100, 54, 's'),
                new MapProp("barrel", 200, 440, 60, 60, 'e'),
                new MapProp("planters", 560, 478, 170, 44, 'n'),
                new MapProp("ferns", 1200, 420, 110, 100, 'e'),
                new MapProp("woodpile", 80, 600, 110, 80, 'e'),
                new MapProp("tank", 80, 1020, 74, 56, 'e'),
                new MapProp("doghouse", 440, 1000, 100, 80, 'w'),
                new MapProp("bigtree", 640, 620, 120, 110, 'n'),
                new MapProp("bigtree", 900, 980, 120, 110, 'n'),
                new MapProp("haystack", 880, 640, 130, 100, 'w'),
                new MapProp("cart", 650, 1000, 150, 80, 'n'),
                new MapProp("coop", 1140, 600, 140, 90, 's'),
                new MapProp("trough", 1250, 880, 200, 50, 'n'),
                new MapProp("shed", 1500, 600, 150, 120, 's'),
                new MapProp("woodpile", 1160, 1000, 110, 80, 'e'),
                new MapProp("tank", 1480, 1050, 74, 56, 'w'),
                // --- decorative only (3) ---
                new MapProp("stove", 950, 50, 110, 60),
                new MapProp("well", 260, 820, 90, 90),
                new MapProp("clothesline", 200, 590, 160, 20),
            },
        };
    }
}
