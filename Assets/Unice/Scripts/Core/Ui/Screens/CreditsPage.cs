using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The credits inside a menu panel: the photo of Aunt Unice, who gave the game its name, then the dedication and the author.
    /// It scrolls by itself, slowly; touching or dragging it hands the scroll to the player for good.
    /// </summary>
    sealed class CreditsPage : UiPart
    {
        /// <summary>Panel pixels per second; slow enough to read.</summary>
        const float ScrollSpeed = 28;
        /// <summary>Resources/Art/<c>tia_unice</c>: the framed photo (png). Without it the credits show the text only.</summary>
        const string PhotoFile = "Art/tia_unice";

        readonly ScrollView scroll;
        float offset;
        bool byHand;

        public CreditsPage(Ui ui, VisualElement panel) : base(ui)
        {
            scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("mscroll"); scroll.AddToClassList("credits-scroll");
            panel.Add(scroll);
            var content = scroll.contentContainer;
            content.AddToClassList("credits");

            var photo = Resources.Load<Texture2D>(PhotoFile);
            if (photo != null)
            {
                var frame = AddElement(content, "credits-photo");
                frame.style.backgroundImage = new StyleBackground(photo);
            }
            LocalizedLabel(content, "credits.tia_name", "credits-name");
            LocalizedLabel(content, "credits.tia_text", "credits-text");
            AddElement(content, "credits-rule");
            LocalizedLabel(content, "credits.made_by", "credits-role");
            AddLabel(content, "Bruno Carvalho", "credits-name");
            AddElement(content, "credits-rule");
            LocalizedLabel(content, "credits.thanks", "credits-text");

            // a touch or a drag takes over: from then on the scroll is the player's
            scroll.RegisterCallback<PointerDownEvent>(_ => byHand = true, TrickleDown.TrickleDown);
            scroll.RegisterCallback<WheelEvent>(_ => byHand = true, TrickleDown.TrickleDown);
        }

        /// <summary>Back at the top, scrolling by itself again (each time the page opens).</summary>
        public void Begin()
        {
            byHand = false; offset = 0;
            scroll.scrollOffset = Vector2.zero;
        }

        /// <summary>Per frame while the credits show.</summary>
        public void Frame(float dt)
        {
            if (byHand) return;
            float end = scroll.contentContainer.layout.height - scroll.contentViewport.layout.height;
            if (end <= 0) return;   // not laid out yet
            offset += Mathf.Min(dt, .1f) * ScrollSpeed;
            scroll.scrollOffset = new Vector2(0, Mathf.Min(offset, end));
            if (offset >= end) byHand = true;   // the end: it stays there for the player to read
        }
    }
}
