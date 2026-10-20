using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>The story list: every story sits on a real map and the doll story comes first.</summary>
    [TestFixture]
    public class StoriesTests
    {
        [Test]
        public void All_EveryStoryHasARealMap()
        {
            foreach (var story in Stories.All) CollectionAssert.Contains(Maps.AllIds, story.mapId);
        }

        [Test]
        public void All_IdsAreUnique()
        {
            Assert.AreEqual(Stories.All.Length, Stories.All.Select(story => story.id).Distinct().Count());
        }

        [Test]
        public void All_TheDollStoryComesFirstOnTheHouseAndIsReleased()
        {
            var first = Stories.All[0];

            Assert.AreEqual(Stories.DollId, first.id);
            Assert.AreEqual(MapHouse.Id, first.mapId);
            Assert.IsTrue(first.released);
        }

        [Test]
        public void All_AStoryWithAVideoSaysHowLongTheMatchWaitsForIt()
        {
            foreach (var story in Stories.All.Where(story => story.hasIntroVideo)) Assert.Greater(story.introSeconds, 0, story.id);
        }

        [Test]
        public void All_AStoryWithAVideoSaysHowLongTheMatchWaitsForIt()
        {
            foreach (var story in Stories.All.Where(story => story.hasIntroVideo)) Assert.Greater(story.introSeconds, 0, story.id);
        }

        [Test]
        public void ForMap_FindsTheStoryOfAMap()
        {
            Assert.AreEqual(Stories.DollId, Stories.ForMap(MapHouse.Id).id);
        }

        [Test]
        public void ForMap_UnknownMap_ReturnsNull()
        {
            Assert.IsNull(Stories.ForMap("nowhere"));
        }

        [Test]
        public void All_IntroVideosArePlainMp4FileNames()
        {
            foreach (var story in Stories.All.Where(story => story.hasIntroVideo))
            {
                StringAssert.EndsWith(".mp4", story.introVideo, story.id);
                Assert.AreEqual(story.introVideo, System.IO.Path.GetFileName(story.introVideo), story.id + " is a file name, not a path");
            }
        }

        [Test]
        public void All_TheDollStoryHasAnIntroVideo()
        {
            Assert.IsTrue(Stories.ForMap(MapHouse.Id).hasIntroVideo);
            Assert.IsFalse(new Story("x", MapHouse.Id, released: true).hasIntroVideo);
        }
    }
}
