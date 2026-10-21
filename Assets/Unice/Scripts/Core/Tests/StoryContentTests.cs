using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// What a story needs besides its map: texts in every language, pictures that exist.
    /// Runs over every story in <see cref="Stories.All"/>, so a new story (docs/stories/README.md) is checked as soon as it is registered.
    /// </summary>
    [TestFixture]
    public class StoryContentTests
    {
        static IEnumerable<Story> AllStories() => Stories.All;

        static bool HasText(string key) => !Localization.Get(key).Contains("missing");

        [TestCaseSource(nameof(AllStories))]
        public void Story_HasANameInEveryLanguage(Story story)
        {
            foreach (Language language in System.Enum.GetValues(typeof(Language)))
            {
                Localization.Set(language);
                Assert.IsTrue(HasText("stories." + story.id + ".name"), story.id + " / " + language);
            }
        }

        [TestCaseSource(nameof(AllStories))]
        public void ReleasedStory_HasABlurb(Story story)
        {
            if (!story.released) Assert.Pass("not released: the card says 'coming soon'");

            Assert.IsTrue(HasText("stories." + story.id + ".blurb"));
        }

        [TestCaseSource(nameof(AllStories))]
        public void Story_CardArtExists(Story story)
        {
            if (story.cardArt == null) Assert.Pass("plain card");

            Assert.IsNotNull(Resources.Load<Texture2D>("Art/" + story.cardArt), "Resources/Art/" + story.cardArt + " is missing");
        }
    }
}
