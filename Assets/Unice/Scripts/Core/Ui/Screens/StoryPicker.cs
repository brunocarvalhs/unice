using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// "Choose the story": one card per <see cref="Story"/>, each on its own map. Stories that aren't finished show as
    /// "coming soon" (still playable in dev builds, like the unfinished maps in the waiting room).
    /// </summary>
    sealed class StoryPicker : UiPart
    {
        /// <summary>Fills <paramref name="panel"/> with the cards and a back button.</summary>
        /// <param name="onPick">Called with the story the player taps.</param>
        public StoryPicker(Ui ui, VisualElement panel, Action<Story> onPick, Action onBack) : base(ui)
        {
            var row = AddElement(panel, "scards");
            foreach (var story in Stories.All) AddCard(row, story, onPick);
            LocalizedButton(panel, "menu.back", onBack, "mbtn", "mbtn-back");
        }

        void AddCard(VisualElement row, Story story, Action<Story> onPick)
        {
            bool playable = story.released || Debug.isDebugBuild;
            var card = AddButton(row, "", () => onPick(story), "scard");
            card.SetEnabled(playable);
            var picture = IgnorePointer(AddElement(card, "scard-art"));
            string art = story.cardArt;
            if (art != null) picture.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Art/" + art));
            IgnorePointer(LocalizedLabel(card, "stories." + story.id + ".name", "scard-name"));
            IgnorePointer(LocalizedLabel(card, story.released ? "stories." + story.id + ".blurb" : "stories.soon", "scard-blurb"));
        }
    }
}
