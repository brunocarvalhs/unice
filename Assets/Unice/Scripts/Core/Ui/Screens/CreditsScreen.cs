using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The credits: the story of the game, a tribute to the aunt who inspired it (with her photo) and who made what, as text that
    /// scrolls up by itself and stops at the end. Holding a finger or the mouse button speeds it up; the back button leaves.
    /// </summary>
    sealed class CreditsScreen : UiPart
    {
        /// <summary>How fast the text climbs, in panel pixels per second, and how many times faster while it is held down.</summary>
        const float ScrollSpeed = 90, HeldSpeedUp = 4;
        /// <summary>Where the last line comes to rest, as a fraction of the screen height from the top.</summary>
        const float RestFraction = .55f;
        /// <summary>The framed photo's size in panel pixels (the picture is 721 × 1001).</summary>
        const float PhotoWidth = 560, PhotoHeight = PhotoWidth * 1001 / 721;
        const string PhotoArt = "Art/tia_moldura";

        public readonly VisualElement Root;
        readonly VisualElement view, scroll;
        readonly Action onDone;
        float elapsed;
        bool held, started;

        /// <summary>Builds the text column inside <paramref name="parent"/>, with a back button.</summary>
        /// <param name="onDone">Called when the back button is pressed (the menu goes back to its main page).</param>
        public CreditsScreen(Ui ui, VisualElement parent, Action onDone) : base(ui)
        {
            this.onDone = onDone;
            Root = AddElement(parent, "credits");
            view = IgnorePointer(AddElement(Root, "credits-view"));
            scroll = IgnorePointer(AddElement(view, "credits-scroll"));

            LocalizedLabel(scroll, "credits.title", "credits-title");
            LocalizedLabel(scroll, "credits.tagline", "credits-tagline");

            LocalizedLabel(scroll, "credits.h_story", "credits-h");
            for (int i = 1; i <= 3; i++) LocalizedLabel(scroll, "credits.story" + i, "credits-p");

            var photo = AddElement(scroll, "credits-photo");
            var texture = Resources.Load<Texture2D>(PhotoArt);
            if (texture != null) photo.style.backgroundImage = new StyleBackground(texture);
            photo.style.width = PhotoWidth; photo.style.height = PhotoHeight;

            LocalizedLabel(scroll, "credits.h_tribute", "credits-h");
            LocalizedLabel(scroll, "credits.tribute_to", "credits-to");
            for (int i = 1; i <= 3; i++) LocalizedLabel(scroll, "credits.tribute" + i, "credits-p");
            LocalizedLabel(scroll, "credits.signature", "credits-sign");

            LocalizedLabel(scroll, "credits.h_credits", "credits-h");
            for (int i = 1; i <= 6; i++) LocalizedLabel(scroll, "credits.c" + i, "credits-line");
            LocalizedLabel(scroll, "credits.thanks", "credits-thanks");

            LocalizedButton(Root, "menu.back", () => this.onDone(), "mbtn", "mexit");
            Root.RegisterCallback<PointerDownEvent>(_ => held = true);
            Root.RegisterCallback<PointerUpEvent>(_ => held = false);
            Root.RegisterCallback<PointerLeaveEvent>(_ => held = false);
        }

        /// <summary>Starts over from the top (each time the credits page opens); the text stays hidden until the first layout is known.</summary>
        public void Begin()
        {
            elapsed = 0; held = false; started = false;
            scroll.style.opacity = 0;
        }

        /// <summary>Per frame while showing: the text climbs, and rests when the last line reaches <see cref="RestFraction"/> of the screen.</summary>
        public void Frame(float dt)
        {
            float viewHeight = view.resolvedStyle.height, contentHeight = scroll.resolvedStyle.height;
            if (float.IsNaN(viewHeight) || viewHeight <= 0 || float.IsNaN(contentHeight) || contentHeight <= 0) return;   // not laid out yet
            if (!started) { started = true; scroll.style.opacity = 1; }
            elapsed += dt * (held ? HeldSpeedUp : 1);
            float y = Mathf.Max(RestFraction * viewHeight - contentHeight, viewHeight - elapsed * ScrollSpeed);
            scroll.style.translate = new Translate(0, y);
        }
    }
}
