using NUnit.Framework;

namespace Unice
{
    /// <summary>When the Unice's player gets the intro comic and how many panels are on the page.</summary>
    [TestFixture]
    public class HideComicTests
    {
        Game game;
        Character unice, hider;

        [SetUp]
        public void SetUp()
        {
            game = MatchFixture.NewMatch();
            unice = game.unice;
            hider = MatchFixture.PlainHider(game);
            game.phase = Phase.Hide; game.round = 1; game.timeLeft = game.hideTime;   // the opening video is over
        }

        [Test]
        public void IsWanted_UniceInTheFirstHidingPhaseOfAStoryWithOnlyAComic_True()
        {
            game.mapId = MapLobisomem.Id;

            Assert.IsTrue(HideComic.IsWanted(game, unice, seenBefore: true));
        }

        [Test]
        public void IsWanted_AHider_False()
        {
            Assert.IsFalse(HideComic.IsWanted(game, hider, seenBefore: false));
        }

        [Test]
        public void IsWanted_NoCharacterYet_False()
        {
            Assert.IsFalse(HideComic.IsWanted(game, null, seenBefore: false));
        }

        [Test]
        public void IsWanted_HuntingPhase_False()
        {
            game.phase = Phase.Hunt;

            Assert.IsFalse(HideComic.IsWanted(game, unice, seenBefore: false));
        }

        [Test]
        public void IsWanted_LaterRoundAlreadySeen_False()
        {
            game.round = 3;

            Assert.IsFalse(HideComic.IsWanted(game, unice, seenBefore: true));
        }

        [Test]
        public void IsWanted_LaterRoundNeverSeen_True()
        {
            game.mapId = MapLobisomem.Id;
            game.round = 3;

            Assert.IsTrue(HideComic.IsWanted(game, unice, seenBefore: false));
        }

        [Test]
        public void IsWanted_MapOfNoStory_False()
        {
            game.mapId = "nowhere";

            Assert.IsFalse(HideComic.IsWanted(game, unice, seenBefore: false));
        }

        [TestCase(30f, 30f, 1)]
        [TestCase(30f, 25f, 1)]
        [TestCase(30f, 24f, 2)]
        [TestCase(30f, 12f, 4)]
        [TestCase(30f, 6f, 5)]
        [TestCase(30f, 0f, 5)]
        public void PanelsShown_SpreadsOverTheHidingTime(float hideTime, float timeLeft, int expected)
        {
            Assert.AreEqual(expected, HideComic.PanelsShown(5, hideTime, timeLeft));
        }

        [Test]
        public void PanelsShown_TimeLeftOutsideTheRange_StaysOnThePage()
        {
            Assert.AreEqual(1, HideComic.PanelsShown(5, 30, 99));
            Assert.AreEqual(5, HideComic.PanelsShown(5, 30, -5));
            Assert.AreEqual(5, HideComic.PanelsShown(5, 0, 0));
        }

        [Test]
        public void PanelsShown_FewerPanels_StillEndsOnTheLastOne()
        {
            Assert.AreEqual(1, HideComic.PanelsShown(3, 30, 30));
            Assert.AreEqual(3, HideComic.PanelsShown(3, 30, 5));
        }

        // ---------- the opening video ----------

        [Test]
        public void IsWanted_TheOpeningVideo_EveryoneIncludingAHider()
        {
            game.timeLeft = game.hideTime + 10;

            Assert.IsTrue(HideComic.IsWanted(game, hider, seenBefore: true));
            Assert.IsTrue(HideComic.IsWanted(game, unice, seenBefore: true));
        }

        [Test]
        public void IsWanted_TheDollStoryAfterItsOpening_False()
        {
            // the video was the opening: no comic on top of it, in the first round or the later ones
            Assert.IsFalse(HideComic.IsWanted(game, unice, seenBefore: false));
            game.round = 3;
            Assert.IsFalse(HideComic.IsWanted(game, unice, seenBefore: false));
        }

        [Test]
        public void IsWanted_TheOpeningVideoButNoCharacterYet_False()
        {
            game.timeLeft = game.hideTime + 10;

            Assert.IsFalse(HideComic.IsWanted(game, null, seenBefore: false));
        }

        [Test]
        public void MediaFor_TheDollStory_IsTheVideoInTheOpeningUntilItFails()
        {
            var story = Stories.ForMap(game.mapId);

            Assert.AreEqual(IntroMedia.Video, HideComic.MediaFor(story, videoFailed: false, opening: true));
            Assert.AreEqual(IntroMedia.Comic, HideComic.MediaFor(story, videoFailed: true, opening: true), "a missing file falls back to the comic");
        }

        [Test]
        public void MediaFor_AfterTheOpening_IsTheComicEvenWithAVideo()
        {
            Assert.AreEqual(IntroMedia.Comic, HideComic.MediaFor(Stories.ForMap(game.mapId), videoFailed: false, opening: false));
        }

        [Test]
        public void MediaFor_NoStory_IsNothing()
        {
            Assert.AreEqual(IntroMedia.None, HideComic.MediaFor(null, videoFailed: false, opening: false));
        }

        [Test]
        public void MediaFor_ComicOnlyStory_IsTheComic()
        {
            var story = new Story("x", MapHouse.Id, released: true, comic: new[] { new ComicPanel(null) });

            Assert.AreEqual(IntroMedia.Comic, HideComic.MediaFor(story, videoFailed: false, opening: true));
        }

        [Test]
        public void MediaFor_StoryWithNeither_IsNothing()
        {
            var story = new Story("x", MapHouse.Id, released: true);

            Assert.AreEqual(IntroMedia.None, HideComic.MediaFor(story, videoFailed: false, opening: true));
            Assert.IsNull(HideComic.VideoPath(story));
        }

        [Test]
        public void VideoPath_IsUnderTheVideosFolder()
        {
            Assert.AreEqual("Videos/doll.mp4", HideComic.VideoPath(Stories.ForMap(game.mapId)));
            Assert.IsNull(HideComic.VideoPath(null));
        }
    }
}
