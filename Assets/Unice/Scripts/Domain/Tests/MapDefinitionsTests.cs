using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>
    /// Rules every map must obey, run over each map in <see cref="Maps.AllIds"/>. A new story's map (see docs/stories/README.md)
    /// is covered the moment it is registered: if one of these fails, the map has a layout bug (a locked-in spawn, a spot nobody can reach…).
    /// </summary>
    [TestFixture]
    public class MapDefinitionsTests
    {
        /// <summary>Eight players is the most a match has; every one of them needs somewhere to hide.</summary>
        const int MinHidingSpots = Rules.MaxPlayers;

        static IEnumerable<string> MapIds() => Maps.AllIds;

        [TearDown]
        public void RestoreHouse() => Map.Load(MapHouse.Build());

        static MapDefinition Load(string mapId)
        {
            var definition = Maps.ById(mapId);
            Map.Load(definition);
            return definition;
        }

        [Test]
        public void AllIds_ListsExactlyTheRegisteredMaps()
        {
            CollectionAssert.AreEquivalent(Maps.All.Keys, Maps.AllIds);
        }

        [TestCaseSource(nameof(MapIds))]
        public void Id_MatchesItsRegistryKey(string mapId)
        {
            Assert.AreEqual(mapId, Maps.All[mapId].id);
        }

        [TestCaseSource(nameof(MapIds))]
        public void Rooms_HaveUniqueIdsAndStayInsideTheWorld(string mapId)
        {
            var definition = Load(mapId);

            Assert.AreEqual(definition.rooms.Length, definition.rooms.Select(room => room.id).Distinct().Count(), "room ids must be unique");
            foreach (var room in definition.rooms)
            {
                Assert.GreaterOrEqual(room.x, 0, room.id); Assert.GreaterOrEqual(room.y, 0, room.id);
                Assert.LessOrEqual(room.x + room.w, definition.worldW, room.id + " right edge");
                Assert.LessOrEqual(room.y + room.h, definition.worldH, room.id + " bottom edge");
            }
        }

        [TestCaseSource(nameof(MapIds))]
        public void Furniture_StaysInsideTheWorld(string mapId)
        {
            var definition = Load(mapId);

            foreach (var prop in definition.furniture)
            {
                Assert.GreaterOrEqual(prop.x, 0, prop.type); Assert.GreaterOrEqual(prop.y, 0, prop.type);
                Assert.LessOrEqual(prop.x + prop.w, definition.worldW, prop.type + " right edge");
                Assert.LessOrEqual(prop.y + prop.h, definition.worldH, prop.type + " bottom edge");
            }
        }

        [TestCaseSource(nameof(MapIds))]
        public void HidingSpots_AreEnoughForEveryPlayer(string mapId)
        {
            Load(mapId);

            Assert.GreaterOrEqual(Map.Spots.Length, MinHidingSpots);
        }

        [TestCaseSource(nameof(MapIds))]
        public void Spawn_IsOnOpenFloor(string mapId)
        {
            Load(mapId);

            Assert.IsTrue(Map.CanStand(Map.SpawnX, Map.SpawnY), "hiders line up here at the start of a round");
        }

        [TestCaseSource(nameof(MapIds))]
        public void Entrance_IsInsideTheMap(string mapId)
        {
            Load(mapId);

            Assert.IsTrue(Map.IsWalkable(Map.Entrance.x, Map.Entrance.y, 0), "the Unice walks in from here");
        }

        [TestCaseSource(nameof(MapIds))]
        public void Entrance_ReachesTheSpawn(string mapId)
        {
            Load(mapId);

            var path = Navigation.FindPath(Map.Entrance.x, Map.Entrance.y, Map.SpawnX, Map.SpawnY);

            Assert.IsNotEmpty(path, "the Unice must be able to walk to where the hiders start");
        }

        [TestCaseSource(nameof(MapIds))]
        public void HidingSpots_CanBeApproachedAndReached(string mapId)
        {
            Load(mapId);

            foreach (var spot in Map.Spots)
            {
                Assert.IsTrue(Map.CanStand(spot.standX, spot.standY), spot.type + " (" + spot.x + "," + spot.y + ") has no free floor to stand on");
                Assert.IsNotEmpty(Navigation.FindPath(Map.SpawnX, Map.SpawnY, spot.standX, spot.standY), spot.type + " (" + spot.x + "," + spot.y + ") can't be reached from the spawn");
            }
        }

        [TestCaseSource(nameof(MapIds))]
        public void EveryRoom_CanBeReachedFromTheSpawn(string mapId)
        {
            Load(mapId);

            foreach (var room in Map.Rooms)
            {
                bool reachable = false;
                // try the room's center, then the points a quarter of the way in, since a piece of furniture may sit in the middle
                foreach (var (fx, fy) in new[] { (.5f, .5f), (.25f, .25f), (.75f, .25f), (.25f, .75f), (.75f, .75f) })
                {
                    float x = room.x + room.w * fx, y = room.y + room.h * fy;
                    if (Map.CanStand(x, y) && Navigation.FindPath(Map.SpawnX, Map.SpawnY, x, y).Count > 0) { reachable = true; break; }
                }
                Assert.IsTrue(reachable, "room " + room.id + " is cut off from the rest of the map");
            }
        }
    }
}
