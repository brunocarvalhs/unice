using NUnit.Framework;

namespace Unice
{
    /// <summary>The sound mix rules that don't need audio: gain per sound, volume and voice choice.</summary>
    [TestFixture]
    public class SfxMixTests
    {
        const int VoiceCount = 12;

        [Test]
        public void GainFor_Heart_QuieterThanGiggle()
        {
            Assert.Less(SfxMix.GainFor("heart"), SfxMix.GainFor("giggle"));
        }

        [Test]
        public void GainFor_UnlistedSound_One()
        {
            Assert.AreEqual(1f, SfxMix.GainFor("tick"));
        }

        [TestCase("heart", 1f, .35f)]
        [TestCase("step", .5f, .3f)]
        [TestCase("tick", 3f, 1f)]
        [TestCase("tick", -2f, 0f)]
        public void VolumeFor_RequestedVolume_IsScaledByTheGainAndKeptInRange(string name, float volume, float expected)
        {
            Assert.AreEqual(expected, SfxMix.VolumeFor(name, volume), .001f);
        }

        [TestCase("giggle", true)]
        [TestCase("sting", true)]
        [TestCase("boom", true)]
        [TestCase("step", false)]
        [TestCase("tick", false)]
        public void IsCritical_OnlySoundsThatTellWhatIsHappening(string name, bool expected)
        {
            Assert.AreEqual(expected, SfxMix.IsCritical(name));
        }

        [TestCase("giggle")]
        [TestCase("sting")]
        [TestCase("boom")]
        public void PickVoice_CriticalSound_ReservedVoiceWithoutAdvancing(string name)
        {
            int nextVoice = 5;

            int voice = SfxMix.PickVoice(SfxMix.IsCritical(name), ref nextVoice, VoiceCount);

            Assert.AreEqual(0, voice, "the reserved voice");
            Assert.AreEqual(5, nextVoice, "the other sounds' turn is kept");
        }

        [Test]
        public void PickVoice_ManyOrdinarySounds_NeverUseReservedVoice()
        {
            int nextVoice = 1;

            for (int i = 0; i < VoiceCount * 3; i++)
                Assert.AreNotEqual(0, SfxMix.PickVoice(SfxMix.IsCritical("step"), ref nextVoice, VoiceCount));
        }

        [Test]
        public void PickVoice_LastVoice_WrapsToFirstUnreserved()
        {
            int nextVoice = VoiceCount - 1;

            SfxMix.PickVoice(false, ref nextVoice, VoiceCount);

            Assert.AreEqual(1, nextVoice);
        }
    }
}
