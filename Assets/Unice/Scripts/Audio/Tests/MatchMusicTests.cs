using System.Linq;
using NUnit.Framework;

namespace Unice
{
    /// <summary>The match's soundtrack loop.</summary>
    [TestFixture]
    public class MatchMusicTests
    {
        [Test]
        public void Loop_Rendered_IsExactlyOneLoopLong()
        {
            var samples = ToneSynth.Render(MatchMusic.Loop());

            Assert.AreEqual(MatchMusic.LoopSeconds, samples.Length / (float)ToneSynth.SampleRate, .06f);
        }

        [Test]
        public void Loop_EveryTone_StartsInsideTheLoopWithAnAudiblePitch()
        {
            foreach (var tone in MatchMusic.Loop())
            {
                Assert.GreaterOrEqual(tone.delay, 0);
                Assert.Less(tone.delay, MatchMusic.LoopSeconds);
                Assert.Greater(tone.frequency, 20);
            }
        }

        [Test]
        public void Loop_Rendered_IsNotSilentAndNeverClips()
        {
            var samples = ToneSynth.Render(MatchMusic.Loop());

            Assert.IsTrue(samples.Any(sample => System.Math.Abs(sample) > .05f));
            Assert.IsTrue(samples.All(sample => System.Math.Abs(sample) < 1));
        }

        [TestCase(0, 392f)]
        [TestCase(7, 784f)]
        [TestCase(-7, 196f)]
        [TestCase(2, 493.88f)]
        public void DegreeFrequency_ScaleDegree_FollowsALydianMixolydianFromG4(int degree, float expected)
        {
            Assert.AreEqual(expected, MatchMusic.DegreeFrequency(degree), .5f);
        }
    }
}
