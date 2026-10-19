using UnityEngine;
using UnityEngine.UIElements;

namespace Unice
{
    /// <summary>The symbols on the round touch buttons and the menu's setting buttons, drawn as vectors (language-neutral, no art needed).</summary>
    enum ActionIcon { Sneak, Laugh, Pass, Door, Eye, SoundOn, SoundOff, Globe }

    static class ActionIcons
    {
        /// <summary>Draws <paramref name="icon"/> centered in the element being painted, scaled to its size.</summary>
        public static void Draw(MeshGenerationContext context, ActionIcon icon, Color color)
        {
            var painter = context.painter2D;
            var rect = context.visualElement.contentRect;
            float centerX = rect.width / 2, centerY = rect.height / 2, size = Mathf.Min(rect.width, rect.height);
            painter.strokeColor = color; painter.fillColor = color; painter.lineWidth = size * .08f; painter.lineCap = LineCap.Round; painter.lineJoin = LineJoin.Round;
            Vector2 At(float dx, float dy) => new Vector2(centerX + dx * size, centerY + dy * size);
            void Dot(Vector2 center, float radius) { painter.BeginPath(); painter.Arc(center, radius * size, Angle.Degrees(0), Angle.Degrees(360)); painter.Fill(); }

            switch (icon)
            {
                case ActionIcon.Sneak:   // two footprints
                    Dot(At(-.14f, -.08f), .11f); Dot(At(.14f, -.08f), .11f);
                    break;
                case ActionIcon.Laugh:   // a smile and two eyes
                    painter.BeginPath(); painter.Arc(At(0, -.02f), size * .22f, Angle.Degrees(15), Angle.Degrees(165)); painter.Stroke();
                    Dot(At(-.16f, -.2f), .035f); Dot(At(.16f, -.2f), .035f);
                    break;
                case ActionIcon.Pass:    // an arrow to the right
                    painter.BeginPath(); painter.MoveTo(At(-.22f, 0)); painter.LineTo(At(.16f, 0)); painter.Stroke();
                    painter.BeginPath(); painter.MoveTo(At(.04f, -.15f)); painter.LineTo(At(.22f, 0)); painter.LineTo(At(.04f, .15f)); painter.Stroke();
                    break;
                case ActionIcon.Door:    // a door with its knob
                    painter.BeginPath(); painter.MoveTo(At(-.15f, .22f)); painter.LineTo(At(-.15f, -.22f)); painter.LineTo(At(.15f, -.22f)); painter.LineTo(At(.15f, .22f)); painter.Stroke();
                    Dot(At(.07f, 0), .025f);
                    break;
                case ActionIcon.Eye:     // an eye (the Unice looking inside)
                    painter.BeginPath(); painter.MoveTo(At(-.24f, 0)); painter.BezierCurveTo(At(-.1f, -.18f), At(.1f, -.18f), At(.24f, 0)); painter.Stroke();
                    painter.BeginPath(); painter.MoveTo(At(-.24f, 0)); painter.BezierCurveTo(At(-.1f, .18f), At(.1f, .18f), At(.24f, 0)); painter.Stroke();
                    Dot(At(0, 0), .07f);
                    break;
                case ActionIcon.SoundOn:   // a speaker with two sound waves
                case ActionIcon.SoundOff:  // the same speaker, crossed out
                    painter.BeginPath(); painter.MoveTo(At(-.3f, -.1f)); painter.LineTo(At(-.18f, -.1f)); painter.LineTo(At(-.02f, -.25f));
                    painter.LineTo(At(-.02f, .25f)); painter.LineTo(At(-.18f, .1f)); painter.LineTo(At(-.3f, .1f)); painter.ClosePath(); painter.Fill();
                    if (icon == ActionIcon.SoundOn)
                    {
                        painter.BeginPath(); painter.Arc(At(-.02f, 0), size * .2f, Angle.Degrees(-45), Angle.Degrees(45)); painter.Stroke();
                        painter.BeginPath(); painter.Arc(At(-.02f, 0), size * .34f, Angle.Degrees(-45), Angle.Degrees(45)); painter.Stroke();
                    }
                    else
                    {
                        painter.BeginPath(); painter.MoveTo(At(.1f, -.14f)); painter.LineTo(At(.32f, .14f)); painter.Stroke();
                        painter.BeginPath(); painter.MoveTo(At(.32f, -.14f)); painter.LineTo(At(.1f, .14f)); painter.Stroke();
                    }
                    break;
                case ActionIcon.Globe:   // a globe: outline, equator and meridian
                    painter.BeginPath(); painter.Arc(At(0, 0), size * .36f, Angle.Degrees(0), Angle.Degrees(360)); painter.Stroke();
                    painter.BeginPath(); painter.MoveTo(At(-.36f, 0)); painter.LineTo(At(.36f, 0)); painter.Stroke();
                    painter.BeginPath(); painter.MoveTo(At(0, -.36f)); painter.BezierCurveTo(At(.24f, -.18f), At(.24f, .18f), At(0, .36f)); painter.Stroke();
                    painter.BeginPath(); painter.MoveTo(At(0, -.36f)); painter.BezierCurveTo(At(-.24f, -.18f), At(-.24f, .18f), At(0, .36f)); painter.Stroke();
                    break;
            }
        }
    }
}
