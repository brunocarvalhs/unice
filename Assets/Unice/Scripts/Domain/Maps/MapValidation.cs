using System.Collections.Generic;

namespace Unice
{
    /// <summary>What part of a map a <see cref="MapProblem"/> is about, so a caller (e.g. the map editor) can highlight it.</summary>
    public enum MapProblemTarget { Map, Room, Furniture, Spawn, Entry }

    /// <summary>One thing wrong with a map. <see cref="Index"/> is the position in <see cref="MapDefinition.rooms"/>/<c>.furniture</c>
    /// for <see cref="MapProblemTarget.Room"/>/<see cref="MapProblemTarget.Furniture"/>, or -1 when not applicable.</summary>
    public readonly struct MapProblem
    {
        public readonly string Message;
        public readonly MapProblemTarget Target;
        public readonly int Index;

        public MapProblem(string message, MapProblemTarget target, int index = -1) { Message = message; Target = target; Index = index; }
    }

    /// <summary>
    /// The same rules <c>MapDefinitionsTests</c> runs over every registered map (room ids unique and in bounds, furniture in bounds,
    /// enough hiding spots, spawn/entrance walkable, entrance reaches the spawn, every spot and room reachable from the spawn), but
    /// against one given <see cref="MapDefinition"/> instead of <see cref="Maps.AllIds"/> — used by the map editor to validate the
    /// map being edited before it is saved.
    /// </summary>
    public static class MapValidation
    {
        /// <summary>Eight players is the most a match has; every one of them needs somewhere to hide.</summary>
        const int MinHidingSpots = Rules.MaxPlayers;

        /// <summary>
        /// Checks <paramref name="map"/> against every rule and returns what, if anything, is wrong (empty when the map is playable).
        /// Mutates the shared <see cref="Map"/>/<see cref="Navigation"/> state via <see cref="Map.Load"/> — this is required to check
        /// <see cref="Map.Spots"/>, <see cref="Map.CanStand"/> and <see cref="Navigation.FindPath"/> against this exact map. The
        /// caller must reload whatever map should be active afterward (the running game, or the editor's rest state).
        /// </summary>
        public static List<MapProblem> Validate(MapDefinition map)
        {
            var problems = new List<MapProblem>();

            for (int i = 0; i < map.rooms.Length; i++)
            {
                var room = map.rooms[i];
                bool duplicateId = false;
                for (int j = 0; j < i; j++) if (map.rooms[j].id == room.id) { duplicateId = true; break; }
                if (duplicateId) problems.Add(new MapProblem("room id \"" + room.id + "\" is used more than once", MapProblemTarget.Room, i));

                if (room.x < 0) problems.Add(new MapProblem(room.id + " left edge is outside the world", MapProblemTarget.Room, i));
                if (room.y < 0) problems.Add(new MapProblem(room.id + " top edge is outside the world", MapProblemTarget.Room, i));
                if (room.x + room.w > map.worldW) problems.Add(new MapProblem(room.id + " right edge is outside the world", MapProblemTarget.Room, i));
                if (room.y + room.h > map.worldH) problems.Add(new MapProblem(room.id + " bottom edge is outside the world", MapProblemTarget.Room, i));
            }

            for (int i = 0; i < map.furniture.Length; i++)
            {
                var prop = map.furniture[i];
                if (prop.x < 0) problems.Add(new MapProblem(prop.type + " left edge is outside the world", MapProblemTarget.Furniture, i));
                if (prop.y < 0) problems.Add(new MapProblem(prop.type + " top edge is outside the world", MapProblemTarget.Furniture, i));
                if (prop.x + prop.w > map.worldW) problems.Add(new MapProblem(prop.type + " right edge is outside the world", MapProblemTarget.Furniture, i));
                if (prop.y + prop.h > map.worldH) problems.Add(new MapProblem(prop.type + " bottom edge is outside the world", MapProblemTarget.Furniture, i));
            }

            Map.Load(map);

            if (Map.Spots.Length < MinHidingSpots)
                problems.Add(new MapProblem("not enough hiding spots for " + MinHidingSpots + " players (has " + Map.Spots.Length + ")", MapProblemTarget.Map));

            if (!Map.CanStand(Map.SpawnX, Map.SpawnY))
                problems.Add(new MapProblem("spawn is not on open floor (hiders line up here at the start of a round)", MapProblemTarget.Spawn));

            if (!Map.IsWalkable(Map.Entrance.x, Map.Entrance.y, 0))
                problems.Add(new MapProblem("entrance is not inside the map (the Unice walks in from here)", MapProblemTarget.Entry));

            if (Navigation.FindPath(Map.Entrance.x, Map.Entrance.y, Map.SpawnX, Map.SpawnY).Count == 0)
                problems.Add(new MapProblem("the Unice must be able to walk to where the hiders start", MapProblemTarget.Entry));

            foreach (var spot in Map.Spots)
            {
                if (!Map.CanStand(spot.standX, spot.standY))
                    problems.Add(new MapProblem(spot.type + " (" + spot.x + "," + spot.y + ") has no free floor to stand on", MapProblemTarget.Furniture, spot.id));
                else if (Navigation.FindPath(Map.SpawnX, Map.SpawnY, spot.standX, spot.standY).Count == 0)
                    problems.Add(new MapProblem(spot.type + " (" + spot.x + "," + spot.y + ") can't be reached from the spawn", MapProblemTarget.Furniture, spot.id));
            }

            for (int i = 0; i < map.rooms.Length; i++)
            {
                var room = map.rooms[i];
                bool reachable = false;
                // try the room's center, then the points a quarter of the way in, since a piece of furniture may sit in the middle
                foreach (var (fx, fy) in new[] { (.5f, .5f), (.25f, .25f), (.75f, .25f), (.25f, .75f), (.75f, .75f) })
                {
                    float x = room.x + room.w * fx, y = room.y + room.h * fy;
                    if (Map.CanStand(x, y) && Navigation.FindPath(Map.SpawnX, Map.SpawnY, x, y).Count > 0) { reachable = true; break; }
                }
                if (!reachable) problems.Add(new MapProblem("room " + room.id + " is cut off from the rest of the map", MapProblemTarget.Room, i));
            }

            return problems;
        }
    }
}
