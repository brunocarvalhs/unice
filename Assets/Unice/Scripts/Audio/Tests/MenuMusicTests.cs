using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>The menu's theme loop.</summary>
    [TestFixture]
    public class MenuMusicTests
    {
        [Test]
        public void Loop_Rendered_IsExactlyOneLoopLong()
        {
            var samples = ToneSynth.Render(MenuMusic.Loop());

            Assert.AreEqual(MenuMusic.LoopSeconds, samples.Length / (float)ToneSynth.SampleRate, .06f);
        }

        [Test]
        public void Loop_EveryTone_StartsInsideTheLoopWithAnAudiblePitch()
        {
            foreach (var tone in MenuMusic.Loop())
            {
                Assert.GreaterOrEqual(tone.delay, 0);
                Assert.Less(tone.delay, MenuMusic.LoopSeconds);
                Assert.Greater(tone.frequency, 20);
            }
        }

        [Test]
        public void Loop_Rendered_IsNotSilentAndNeverClips()
        {
            var samples = ToneSynth.Render(MenuMusic.Loop());

            Assert.IsTrue(samples.Any(sample => System.Math.Abs(sample) > .05f));
            Assert.IsTrue(samples.All(sample => System.Math.Abs(sample) < 1));
        }

        [TestCase(0, 293.66f)]
        [TestCase(7, 587.33f)]
        [TestCase(-1, 261.63f)]   // C: the flat seventh below the root
        [TestCase(-7, 146.83f)]
        public void DegreeFrequency_ScaleDegree_FollowsDMixolydian(int degree, float expected)
        {
            Assert.AreEqual(expected, MenuMusic.DegreeFrequency(degree), .5f);
        }
    }
}
