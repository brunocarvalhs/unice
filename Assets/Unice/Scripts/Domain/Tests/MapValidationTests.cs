using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>
    /// <see cref="MapValidation.Validate"/> against the three shipped maps (a cross-check that it agrees with the already-passing
    /// <c>MapDefinitionsTests</c>) and one hand-crafted broken map per rule.
    /// </summary>
    [TestFixture]
    public class MapValidationTests
    {
        static IEnumerable<string> MapIds() => Maps.AllIds;

        [TearDown]
        public void RestoreHouse() => Map.Load(MapHouse.Build());

        [TestCaseSource(nameof(MapIds))]
        public void ShippedMaps_HaveNoProblems(string mapId)
        {
            var problems = MapValidation.Validate(Maps.ById(mapId));

            CollectionAssert.IsEmpty(problems, string.Join("; ", problems.Select(p => p.Message)));
        }

        [Test]
        public void DuplicateRoomId_IsReported()
        {
            var map = MapHouse.Build();
            map.rooms[1].id = map.rooms[0].id;

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Room && p.Index == 1));
        }

        [Test]
        public void RoomOutsideTheWorld_IsReported()
        {
            var map = MapHouse.Build();
            map.rooms[0].x = -100;

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Room && p.Index == 0));
        }

        [Test]
        public void FurnitureOutsideTheWorld_IsReported()
        {
            var map = MapHouse.Build();
            map.furniture[0].x = map.worldW + 1000;

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Furniture && p.Index == 0));
        }

        [Test]
        public void NotEnoughHidingSpots_IsReported()
        {
            var map = MapHouse.Build();
            map.furniture = Array.Empty<MapProp>();

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Map));
        }

        [Test]
        public void SpawnNotOnOpenFloor_IsReported()
        {
            var map = MapHouse.Build();
            map.spawnX = -500; map.spawnY = -500;

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Spawn));
        }

        [Test]
        public void EntranceOutsideTheMap_IsReported()
        {
            var map = MapHouse.Build();
            map.entryX = -500; map.entryY = -500;

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Entry));
        }

        [Test]
        public void EntranceCannotReachSpawn_IsReported()
        {
            // two rooms far enough apart that no door or node links them: the entrance (room a) can't walk to the spawn (room b)
            var map = TwoDisconnectedRooms(entryX: 75, entryY: 75, spawnX: 325, spawnY: 325);

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Entry));
        }

        [Test]
        public void HidingSpotCannotBeReached_IsReported()
        {
            // spawn/entrance share room a; the one hiding spot sits in the disconnected room b
            var map = TwoDisconnectedRooms(entryX: 75, entryY: 75, spawnX: 75, spawnY: 100,
                furniture: new[] { new MapProp("bed", 280, 280, 40, 40, 's') });

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Furniture && p.Index == 0));
        }

        [Test]
        public void RoomCannotBeReachedFromSpawn_IsReported()
        {
            // spawn/entrance share room a; room b has nothing in it, but is still cut off
            var map = TwoDisconnectedRooms(entryX: 75, entryY: 75, spawnX: 75, spawnY: 100);

            var problems = MapValidation.Validate(map);

            Assert.IsTrue(problems.Any(p => p.Target == MapProblemTarget.Room && p.Index == 1));
        }

        /// <summary>Two 150x150 rooms 100px apart (too far for a door-less grid link): room "a" at the top-left, room "b" at the bottom-right.</summary>
        static MapDefinition TwoDisconnectedRooms(float entryX, float entryY, float spawnX, float spawnY, MapProp[] furniture = null) => new MapDefinition
        {
            id = "test-disconnected",
            worldW = 400, worldH = 400,
            groundTint = "#000000",
            rooms = new[] { new Room("a", 0, 0, 150, 150), new Room("b", 250, 250, 150, 150) },
            verticalDoors = Array.Empty<MapDoor>(),
            horizontalDoors = Array.Empty<MapDoor>(),
            furniture = furniture ?? Array.Empty<MapProp>(),
            entryX = entryX, entryY = entryY, spawnX = spawnX, spawnY = spawnY,
        };
    }
}
