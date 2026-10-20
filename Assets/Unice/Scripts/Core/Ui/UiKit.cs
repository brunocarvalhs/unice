using System;
using UnityEngine.UIElements;

namespace Unice
{
    /// <summary>
    /// Tiny UI Toolkit builders shared by every screen (import with <c>using static Unice.UiKit;</c>).
    /// Each "Add" creates an element, gives it the USS classes, appends it to <c>parent</c> and returns it.
    /// </summary>
    static class UiKit
    {
        public static T Add<T>(VisualElement parent, T element, string[] classes) where T : VisualElement
        {
            foreach (var cssClass in classes) element.AddToClassList(cssClass);
            parent.Add(element);
            return element;
        }

        public static VisualElement AddElement(VisualElement parent, params string[] classes) => Add(parent, new VisualElement(), classes);
        public static Label AddLabel(VisualElement parent, string text, params string[] classes) => Add(parent, new Label(text), classes);
        public static Button AddButton(VisualElement parent, string text, Action onClick, params string[] classes) => Add(parent, new Button(onClick) { text = text }, classes);

        /// <summary>A button that shows only a vector icon, drawn in the button's own text color (so hover and focus tint it).</summary>
        public static Button AddGlyphButton(VisualElement parent, ActionIcon icon, Action onClick, params string[] classes)
        {
            var button = AddButton(parent, "", onClick, classes);
            var glyph = IgnorePointer(AddElement(button, "hud-glyph"));
            glyph.generateVisualContent += context => ActionIcons.Draw(context, icon, button.resolvedStyle.color);
            button.RegisterCallback<PointerEnterEvent>(e => glyph.MarkDirtyRepaint()); button.RegisterCallback<PointerLeaveEvent>(e => glyph.MarkDirtyRepaint());
            button.RegisterCallback<FocusInEvent>(e => glyph.MarkDirtyRepaint()); button.RegisterCallback<FocusOutEvent>(e => glyph.MarkDirtyRepaint());
            return button;
        }

        public static TextField AddTextField(VisualElement parent, string value, int maxLength, params string[] classes)
        {
            var field = new TextField { value = value, maxLength = maxLength };
            field.AddToClassList("field");
            return Add(parent, field, classes);
        }

        /// <summary>A dimmed full-screen overlay with a scrollable sheet in the middle; returns the sheet.</summary>
        public static VisualElement AddSheet(VisualElement parent, out VisualElement overlay)
        {
            overlay = AddElement(parent, "overlay");
            var scroll = new ScrollView(ScrollViewMode.Vertical); scroll.AddToClassList("sheet-scroll"); overlay.Add(scroll);
            return AddElement(scroll, "sheet");
        }

        /// <summary>A vertical scroll (USS class <c>scrollClass</c>) whose content gets <c>contentClass</c>; returns the content, where children go.</summary>
        public static VisualElement AddScroll(VisualElement parent, string scrollClass, string contentClass)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList(scrollClass); parent.Add(scroll);
            if (contentClass != null) scroll.contentContainer.AddToClassList(contentClass);
            return scroll.contentContainer;
        }

        /// <summary>Lets touches and clicks pass through the element (for overlays and labels drawn over the game).</summary>
        public static T IgnorePointer<T>(T element) where T : VisualElement { element.pickingMode = PickingMode.Ignore; return element; }

        /// <summary>Shows or hides (display: none). Only writes the style when it changes, since this runs every step.</summary>
        public static void Show(VisualElement element, bool visible)
        {
            var display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (element.style.display != display) element.style.display = display;
        }

        /// <summary>Sets the text only when it changes (avoids re-layout every step).</summary>
        public static void SetText(Label label, string text) { if (label.text != text) label.text = text; }
    }
}
