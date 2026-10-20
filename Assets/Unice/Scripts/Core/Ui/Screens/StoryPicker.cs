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
            // the card is laid out in Resources/UI/Screens/StoryCard.uxml (UI Builder)
            var card = (Button)ui.LoadScreen("StoryCard", row);
            card.clicked += () => onPick(story);
            card.SetEnabled(playable);
            var picture = IgnorePointer(card.Q("art"));
            string art = story.cardArt;
            if (art != null) picture.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Art/" + art));
            IgnorePointer(ui.Localize(card.Q<Label>("name"), "stories." + story.id + ".name"));
            IgnorePointer(ui.Localize(card.Q<Label>("blurb"), story.released ? "stories." + story.id + ".blurb" : "stories.soon"));
        }
    }
}
