using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The cordel poster behind the menu, full bleed (under the notch too) and alive: the sun turns and breathes, birds fly across
    /// flapping, stars twinkle and the cacti sway. Shapes and colors are in unice.uss; this only moves them.
    /// </summary>
    sealed class CordelBackdrop : UiPart
    {
        const int RayCount = 18;

        public readonly VisualElement Root;
        readonly VisualElement sun, rays;
        readonly (VisualElement element, float speed, float phase, float flap)[] birds;
        readonly (VisualElement element, float speed, float phase)[] stars;
        readonly (VisualElement element, float speed, float phase)[] cacti;

        public CordelBackdrop(Ui ui, VisualElement parent) : base(ui)
        {
            Root = IgnorePointer(AddElement(parent, "menu-bg"));
            sun = IgnorePointer(AddElement(Root, "menu-sun"));
            rays = IgnorePointer(AddElement(Root, "menu-rays"));
            rays.generateVisualContent += PaintRays;
            IgnorePointer(AddElement(Root, "menu-dune"));
            IgnorePointer(AddElement(Root, "menu-ground"));

            VisualElement Decor(string art, params string[] classes)
            {
                var piece = IgnorePointer(AddElement(Root, classes));
                piece.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Art/" + art));
                return piece;
            }
            birds = new[]
            {
                (Decor("cordel_bird", "decor", "decor-bird-a"), .035f, 0.10f, 5.0f),
                (Decor("cordel_bird", "decor", "decor-bird-b"), .024f, 0.55f, 4.0f),
                (Decor("cordel_bird", "decor", "decor-bird-c"), .018f, 0.85f, 3.4f),
            };
            stars = new[]
            {
                (Decor("cordel_star", "decor", "decor-star-a"), 1.6f, 0.0f),
                (Decor("cordel_star", "decor", "decor-star-b"), 1.2f, 2.0f),
                (Decor("cordel_star", "decor", "decor-star-c"), 1.9f, 4.0f),
                (Decor("cordel_star", "decor", "decor-star-d"), 1.4f, 1.0f),
            };
            cacti = new[]
            {
                (Decor("cordel_cactus", "decor", "decor-cactus-l"), .7f, 0.0f),
                (Decor("cordel_cactus", "decor", "decor-cactus-r"), .55f, 2.2f),
            };
            // the cacti sway from their roots
            foreach (var cactus in cacti) cactus.element.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(100));
        }

        /// <summary>Per frame while the menu shows.</summary>
        public void Frame()
        {
            float time = Time.unscaledTime;
            float breath = 1 + .03f * Mathf.Sin(time * .9f);
            sun.style.scale = new Scale(new Vector3(breath, breath, 1));
            rays.style.rotate = new Rotate(Angle.Degrees(time * 4f));
            foreach (var (element, speed, phase, flap) in birds)
            {
                // from just off the left edge to just off the right one, then around again
                float across = Mathf.Repeat(time * speed + phase, 1.3f) - .15f;
                element.style.left = Length.Percent(across * 100);
                element.style.scale = new Scale(new Vector3(1, 1 + .22f * Mathf.Sin(time * flap + phase * 10), 1));
                element.style.translate = new Translate(0, Mathf.Sin(time * .8f + phase * 6) * 10);
            }
            foreach (var (element, speed, phase) in stars)
            {
                float twinkle = .5f + .5f * Mathf.Sin(time * speed + phase);
                element.style.opacity = .35f + .65f * twinkle;
                float size = .8f + .3f * twinkle;
                element.style.scale = new Scale(new Vector3(size, size, 1));
                element.style.rotate = new Rotate(Angle.Degrees(Mathf.Sin(time * .5f + phase) * 12));
            }
            foreach (var (element, speed, phase) in cacti)
                element.style.rotate = new Rotate(Angle.Degrees(Mathf.Sin(time * speed + phase) * 1.4f));
        }

        /// <summary>Thin spokes around the sun, in the element's <c>color</c>.</summary>
        void PaintRays(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            var rect = context.visualElement.contentRect;
            var center = new Vector2(rect.width / 2, rect.height / 2);
            float inner = Mathf.Min(rect.width, rect.height) * .5f, outer = inner * 1.22f;
            painter.strokeColor = context.visualElement.resolvedStyle.color; painter.lineWidth = 4; painter.lineCap = LineCap.Round;
            for (int i = 0; i < RayCount; i++)
            {
                float angle = i * Mathf.PI * 2 / RayCount;
                float reach = i % 2 == 0 ? outer : (inner + outer) / 2;
                var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                painter.BeginPath(); painter.MoveTo(center + direction * (inner + 10)); painter.LineTo(center + direction * reach); painter.Stroke();
            }
        }
    }
}
