using NUnit.Framework;

namespace Unice
{
    /// <summary>How far a sound carries, how loud it is, where it comes from.</summary>
    [TestFixture]
    public class SoundMathTests
    {
        [Test]
        public void Reach_ListenerHidden_Is30PercentLonger()
        {
            Assert.AreEqual(130f, SoundMath.Reach(100, true), .001f);
        }

        [Test]
        public void Reach_ListenerNotHidden_IsTheRange()
        {
            Assert.AreEqual(100f, SoundMath.Reach(100, false), .001f);
        }

        [TestCase(0f, 1f)]
        [TestCase(50f, .25f)]
        [TestCase(100f, 0f)]
        [TestCase(250f, 0f)]
        public void Loudness_AtDistance_FallsWithTheSquareOfCloseness(float distance, float expected)
        {
            Assert.AreEqual(expected, SoundMath.Loudness(distance, 100), .001f);
        }

        [Test]
        public void Loudness_BeyondReachWithAMinimum_NeverDropsBelowIt()
        {
            Assert.AreEqual(.35f, SoundMath.Loudness(500, 100, .35f), .001f);
        }

        [TestCase(-900f, -1f)]
        [TestCase(-150f, -.5f)]
        [TestCase(0f, 0f)]
        [TestCase(300f, 1f)]
        [TestCase(5000f, 1f)]
        public void Pan_SourceToTheSide_IsClampedToTheEars(float dx, float expected)
        {
            Assert.AreEqual(expected, SoundMath.Pan(dx), .001f);
        }

        [TestCase(30f, false, false)]
        [TestCase(30f, true, false)]
        [TestCase(200f, true, false)]
        [TestCase(200f, false, true)]
        public void IsMuffled_OnlyWhenFarAndBehindAWall(float distance, bool lineOfSight, bool expected)
        {
            Assert.AreEqual(expected, SoundMath.IsMuffled(distance, lineOfSight));
        }

        [TestCase(.9f, "app.close.near")]
        [TestCase(.3f, "app.close.mid")]
        [TestCase(.05f, "app.close.far")]
        public void DescribeDistance_ByLoudness_PicksNearMidOrFar(float loudness, string key)
        {
            Assert.AreEqual(Localization.Get(key), SoundMath.DescribeDistance(loudness));
        }

        [TestCase(10f, 10f, "app.side.near")]
        [TestCase(-200f, 10f, "app.side.left")]
        [TestCase(200f, -10f, "app.side.right")]
        [TestCase(10f, -200f, "app.side.above")]
        [TestCase(10f, 200f, "app.side.below")]
        public void DescribeSide_OneAxisDominates_SaysOnlyThatSide(float dx, float dy, string key)
        {
            Assert.AreEqual(Localization.Get(key), SoundMath.DescribeSide(dx, dy));
        }

        [Test]
        public void DescribeSide_BothAxesSimilar_SaysVerticalThenHorizontal()
        {
            string expected = Localization.Get("app.side.above") + ", " + Localization.Get("app.side.left");

            Assert.AreEqual(expected, SoundMath.DescribeSide(-150, -150));
        }
    }
}
