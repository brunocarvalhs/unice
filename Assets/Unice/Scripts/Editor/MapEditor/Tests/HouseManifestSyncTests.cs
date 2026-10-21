using System.IO;
using NUnit.Framework;

namespace Unice
{
    /// <summary>
    /// <see cref="HouseManifestSync.SyncFloors"/> is pure (string in, string out), so these build small fake manifest snippets
    /// rather than going through the file system — except <see cref="RealManifest_WithNoRoomsChanged_RoundTripsByteForByte"/>,
    /// which reads the real shipped file to prove a no-op save would leave it untouched byte for byte.
    /// </summary>
    [TestFixture]
    public class HouseManifestSyncTests
    {
        const string RealManifestPath = "Assets/Unice/Resources/Art/manifest.json";

        static Room[] Rooms(params Room[] rooms) => rooms;

        [Test]
        public void MovedRoom_OnlyItsFloorNumbersChange()
        {
            const string json = "{\"floors\":[" +
                "{\"id\":\"living\",\"file\":\"floor_living\",\"x\":40,\"y\":558,\"w\":520,\"h\":342}," +
                "{\"id\":\"dining\",\"file\":\"floor_dining\",\"x\":584,\"y\":558,\"w\":480,\"h\":342}" +
                "],\"layers\":[{\"name\":\"cape\"}]}";

            string updated = HouseManifestSync.SyncFloors(json, Rooms(new Room("living", 100, 200, 520, 342)));

            const string expected = "{\"floors\":[" +
                "{\"id\":\"living\",\"file\":\"floor_living\",\"x\":100,\"y\":200,\"w\":520,\"h\":342}," +
                "{\"id\":\"dining\",\"file\":\"floor_dining\",\"x\":584,\"y\":558,\"w\":480,\"h\":342}" +
                "],\"layers\":[{\"name\":\"cape\"}]}";
            Assert.AreEqual(expected, updated);
        }

        [Test]
        public void RoomWithNoMatchingFloor_IsANoOp()
        {
            const string json = "{\"floors\":[{\"id\":\"living\",\"file\":\"floor_living\",\"x\":40,\"y\":558,\"w\":520,\"h\":342}]}";

            string updated = HouseManifestSync.SyncFloors(json, Rooms(new Room("sala2", 999, 999, 10, 10)));

            Assert.AreEqual(json, updated);
        }

        [Test]
        public void NoRoomsChanged_ReturnsTheInputUnchanged()
        {
            const string json = "{\"floors\":[{\"id\":\"living\",\"file\":\"floor_living\",\"x\":40,\"y\":558,\"w\":520,\"h\":342}]}";

            string updated = HouseManifestSync.SyncFloors(json, Rooms(new Room("living", 40, 558, 520, 342)));

            Assert.AreEqual(json, updated);
        }

        [Test]
        public void RealManifest_WithNoRoomsChanged_RoundTripsByteForByte()
        {
            string original = File.ReadAllText(RealManifestPath);

            // the real MapHouse.Build() rooms, unchanged: syncing them back should touch nothing
            var rooms = Rooms(
                new Room("master", 40, 40, 440, 350), new Room("bath", 504, 40, 220, 350), new Room("kids", 748, 40, 420, 350),
                new Room("library", 1192, 40, 488, 350), new Room("hall", 40, 414, 1640, 120), new Room("living", 40, 558, 520, 342),
                new Room("dining", 584, 558, 480, 342), new Room("kitchen", 1088, 558, 592, 342), new Room("estufa", 40, 924, 520, 216),
                new Room("entrance", 584, 924, 480, 216), new Room("pantry", 1088, 924, 300, 216), new Room("laundry", 1412, 924, 268, 216));

            string updated = HouseManifestSync.SyncFloors(original, rooms);

            Assert.AreEqual(original, updated);
        }
    }
}
