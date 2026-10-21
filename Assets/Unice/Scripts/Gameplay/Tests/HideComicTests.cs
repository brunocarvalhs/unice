using NUnit.Framework;

namespace Unice
{
    /// <summary>When the story's opening video is wanted on the Unice's screen while the others hide.</summary>
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
            game.phase = Phase.Hide; game.round = 1; game.timeLeft = game.hideTime + 10;   // inside the doll story's opening window
        }

        [Test]
        public void IsWanted_TheOpeningVideo_EveryoneIncludingAHider()
        {
            Assert.IsTrue(HideComic.IsWanted(game, hider));
            Assert.IsTrue(HideComic.IsWanted(game, unice));
        }

        [Test]
        public void IsWanted_AHider_FalseOnceTheOpeningEnds()
        {
            game.timeLeft = game.hideTime;

            Assert.IsFalse(HideComic.IsWanted(game, hider));
        }

        [Test]
        public void IsWanted_AfterTheOpening_FalseEveryRound()
        {
            game.timeLeft = game.hideTime;
            Assert.IsFalse(HideComic.IsWanted(game, unice));

            game.round = 3;
            Assert.IsFalse(HideComic.IsWanted(game, unice));
        }

        [Test]
        public void IsWanted_NoCharacterYet_False()
        {
            Assert.IsFalse(HideComic.IsWanted(game, null));
        }

        [Test]
        public void IsWanted_HuntingPhase_False()
        {
            game.phase = Phase.Hunt;

            Assert.IsFalse(HideComic.IsWanted(game, unice));
        }

        [Test]
        public void IsWanted_MapOfNoStory_False()
        {
            game.mapId = "nowhere";

            Assert.IsFalse(HideComic.IsWanted(game, unice));
        }

        [Test]
        public void IsWanted_StoryWithNoVideo_FalseEvenPastHideTime()
        {
            // lobisomem has no video, so it never raises timeLeft past hideTime (Game.StartRound), but IsWanted stays false on its own too
            game.mapId = MapLobisomem.Id;
            game.timeLeft = game.hideTime + 10;

            Assert.IsFalse(HideComic.IsWanted(game, unice));
        }

        [Test]
        public void MediaFor_TheDollStory_IsTheVideoInTheOpeningUntilItFails()
        {
            var story = Stories.ForMap(game.mapId);

            Assert.AreEqual(IntroMedia.Video, HideComic.MediaFor(story, videoFailed: false, opening: true));
            Assert.AreEqual(IntroMedia.None, HideComic.MediaFor(story, videoFailed: true, opening: true), "a missing file falls back to nothing");
        }

        [Test]
        public void MediaFor_NotOpening_IsNothingEvenWithAVideo()
        {
            Assert.AreEqual(IntroMedia.None, HideComic.MediaFor(Stories.ForMap(game.mapId), videoFailed: false, opening: false));
        }

        [Test]
        public void MediaFor_NoStory_IsNothing()
        {
            Assert.AreEqual(IntroMedia.None, HideComic.MediaFor(null, videoFailed: false, opening: false));
        }

        [Test]
        public void MediaFor_StoryWithNoVideo_IsNothing()
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
