using System;

namespace Unice
{
    /// <summary>
    /// How tall the UI panel is, in USS pixels, for the window it is drawn in. The panel is matched by height, so a taller panel
    /// means smaller controls: phones and tablets keep the 540 px panel (a 64 px button is then ≥ 48 dp), while on a computer the
    /// panel grows with the window so the buttons do not look like a tablet's on a big screen.
    /// </summary>
    public static class PanelScale
    {
        /// <summary>The panel height on phones and tablets, and in a small desktop window.</summary>
        public const int HandheldHeight = 540;
        /// <summary>The panel height of a big desktop window (1.67 times taller than the handheld panel: controls 40 % smaller).</summary>
        public const int DesktopHeight = 900;
        /// <summary>Window heights, in screen pixels, between which the desktop panel grows from <see cref="HandheldHeight"/> to <see cref="DesktopHeight"/>.</summary>
        public const int SmallWindow = 540, BigWindow = 1080;

        /// <summary>The panel height for a window <paramref name="screenHeight"/> pixels tall; <paramref name="handheld"/> = a phone or tablet.</summary>
        public static int ReferenceHeight(bool handheld, int screenHeight)
        {
            if (handheld) return HandheldHeight;
            float grown = Math.Max(0, Math.Min(1, (screenHeight - SmallWindow) / (float)(BigWindow - SmallWindow)));
            return (int)Math.Round(HandheldHeight + (DesktopHeight - HandheldHeight) * grown);
        }
    }
}
