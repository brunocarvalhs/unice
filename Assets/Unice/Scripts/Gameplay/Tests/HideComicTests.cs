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
            game.phase = Phase.Hide; game.round = 1;
        }

        [Test]
        public void IsWanted_UniceInTheFirstHidingPhaseOfTheDollStory_True()
        {
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
    }
}
