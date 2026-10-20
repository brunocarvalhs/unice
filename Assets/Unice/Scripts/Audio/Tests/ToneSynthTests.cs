using System.Linq;
using NUnit.Framework;
using static Unice.ToneSynth;

namespace Unice
{
    /// <summary>The tone synthesizer and the catalog of sounds built from it.</summary>
    [TestFixture]
    public class ToneSynthTests
    {
        [TestCase(Wave.Square, 0f, 1f)]
        [TestCase(Wave.Square, .75f, -1f)]
        [TestCase(Wave.Triangle, 0f, -1f)]
        [TestCase(Wave.Triangle, .25f, 0f)]
        [TestCase(Wave.Triangle, .5f, 1f)]
        [TestCase(Wave.Triangle, .75f, 0f)]
        [TestCase(Wave.Saw, 0f, -1f)]
        [TestCase(Wave.Saw, .5f, 0f)]
        public void WaveAt_PointOfThePeriod_ReturnsTheWaveShape(Wave wave, float cycle, float expected)
        {
            Assert.AreEqual(expected, WaveAt(wave, cycle), .001f);
        }

        [Test]
        public void MakeTone_Arguments_AreStoredOnTheTone()
        {
            var tone = MakeTone(440, .5f, Wave.Saw, .1f, 220, .25f);

            Assert.AreEqual((440f, 220f, .5f, .1f, .25f, Wave.Saw), (tone.frequency, tone.endFrequency, tone.duration, tone.volume, tone.delay, tone.wave));
        }

        [Test]
        public void ToneSequence_ThreeNotes_AreSpacedByTheGap()
        {
            var tones = ToneSequence(new[] { 100f, 200, 300 }, .1f, Wave.Triangle, .05f, .2f, 1);

            CollectionAssert.AreEqual(new[] { 0f, .2f, .4f }, tones.Select(tone => tone.delay).ToArray());
        }

        [Test]
        public void ToneSequence_NoPitchBend_NotesDoNotSlide()
        {
            var tones = ToneSequence(new[] { 100f }, .1f, Wave.Triangle, .05f, .2f, 1);

            Assert.AreEqual(0f, tones[0].endFrequency);
        }

        [Test]
        public void ToneSequence_PitchBend_EachNoteSlidesByThatFactor()
        {
            var tones = ToneSequence(new[] { 100f }, .1f, Wave.Triangle, .05f, .2f, 1.5f);

            Assert.AreEqual(150f, tones[0].endFrequency, .001f);
        }

        [Test]
        public void SampleCount_OneTone_IsItsDurationPlusTheTail()
        {
            int samples = SampleCount(new[] { MakeTone(440, 1, Wave.Square, .1f) });

            Assert.AreEqual((1f + .03f) * SampleRate, samples, 2);
        }

        [Test]
        public void SampleCount_DelayedTone_CountsTheDelay()
        {
            int samples = SampleCount(new[] { MakeTone(440, .5f, Wave.Square, .1f, 0, 1) });

            Assert.AreEqual((1.5f + .03f) * SampleRate, samples, 2);
        }

        [Test]
        public void Render_Tone_StaysWithinMinusOneAndOne()
        {
            var samples = Render(new[] { MakeTone(300, .3f, Wave.Square, 1f) });

            Assert.IsTrue(samples.All(sample => sample >= -1 && sample <= 1));
        }

        [Test]
        public void Render_Tone_StartsLoudAndEndsAlmostSilent()
        {
            var samples = Render(new[] { MakeTone(300, .3f, Wave.Square, .2f) });
            int early = (int)(.01f * SampleRate), late = (int)(.28f * SampleRate);

            float loudStart = samples.Skip(0).Take(early).Max(sample => System.Math.Abs(sample));
            float quietEnd = samples.Skip(late).Take(early).Max(sample => System.Math.Abs(sample));

            Assert.Greater(loudStart, quietEnd * 10);
        }

        [Test]
        public void Render_DelayedTone_IsSilentBeforeItsStart()
        {
            var samples = Render(new[] { MakeTone(300, .1f, Wave.Square, .2f, 0, .5f) });

            Assert.IsTrue(samples.Take((int)(.49f * SampleRate)).All(sample => sample == 0));
        }

        [Test]
        public void Render_SlidingTone_ProducesSound()
        {
            var samples = Render(new[] { MakeTone(900, .5f, Wave.Saw, .05f, 90) });

            Assert.IsTrue(samples.Any(sample => sample != 0));
        }

        [Test]
        public void Render_TwoOverlappingTones_AreMixedNotReplaced()
        {
            var one = Render(new[] { MakeTone(300, .1f, Wave.Triangle, .05f) });
            var both = Render(new[] { MakeTone(300, .1f, Wave.Triangle, .05f), MakeTone(300, .1f, Wave.Triangle, .05f) });

            Assert.Greater(both.Max(), one.Max());
        }

        // ---------- the catalog ----------

        [TestCase("giggle")] [TestCase("heart")] [TestCase("cane")] [TestCase("creak")] [TestCase("sting")] [TestCase("swish")]
        [TestCase("step")] [TestCase("tick")] [TestCase("clock")] [TestCase("boom")] [TestCase("bad")] [TestCase("win")] [TestCase("lose")]
        public void SfxCatalog_EverySoundTheGameAsksFor_Exists(string name)
        {
            Assert.IsTrue(SfxCatalog.Sounds.ContainsKey(name));
        }

        [Test]
        public void SfxCatalog_EverySound_RendersSomethingAudibleWithinRange()
        {
            foreach (var sound in SfxCatalog.Sounds)
            {
                var samples = Render(sound.Value);
                Assert.IsTrue(samples.Any(sample => System.Math.Abs(sample) > .001f), sound.Key + " is silent");
                Assert.IsTrue(samples.All(sample => sample >= -1 && sample <= 1), sound.Key + " clips");
            }
        }
    }
}
