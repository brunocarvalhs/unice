using NUnit.Framework;

namespace Unice
{
    /// <summary>The UI panel keeps its size on handhelds and grows with a desktop window.</summary>
    [TestFixture]
    public class PanelScaleTests
    {
        [TestCase(0)]
        [TestCase(1080)]
        [TestCase(2400)]
        public void ReferenceHeight_Handheld_IsAlwaysTheSame(int screenHeight)
        {
            Assert.AreEqual(PanelScale.HandheldHeight, PanelScale.ReferenceHeight(true, screenHeight));
        }

        [TestCase(300, PanelScale.HandheldHeight)]
        [TestCase(PanelScale.SmallWindow, PanelScale.HandheldHeight)]
        [TestCase(PanelScale.BigWindow, PanelScale.DesktopHeight)]
        [TestCase(1800, PanelScale.DesktopHeight)]
        public void ReferenceHeight_Desktop_StaysInTheRange(int screenHeight, int expected)
        {
            Assert.AreEqual(expected, PanelScale.ReferenceHeight(false, screenHeight));
        }

        [Test]
        public void ReferenceHeight_Desktop_GrowsWithTheWindow()
        {
            int previous = 0;
            for (int height = 400; height <= 1400; height += 50)
            {
                int panel = PanelScale.ReferenceHeight(false, height);
                Assert.GreaterOrEqual(panel, previous, "window " + height);
                previous = panel;
            }
            Assert.AreEqual(720, PanelScale.ReferenceHeight(false, 810), "halfway between the two");
        }
    }
}
