using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Unice
{
    /// <summary>
    /// What a story needs besides its map: texts in every language, pictures that exist, sound captions for the comic.
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
        public void Comic_EveryPanelHasACaptionInEveryLanguage(Story story)
        {
            for (int panel = 1; panel <= story.comic.Length; panel++)
                foreach (Language language in System.Enum.GetValues(typeof(Language)))
                {
                    Localization.Set(language);
                    Assert.IsTrue(HasText("comic." + story.id + "." + panel), "panel " + panel + " / " + language);
                }
        }

        [TestCaseSource(nameof(AllStories))]
        public void Comic_EveryPictureExists(Story story)
        {
            var files = story.comic.Select(panel => panel.backdrop)
                .Concat(story.comic.SelectMany(panel => panel.figures).Where(figure => figure.kind == ComicFigureKind.Sprite).Select(figure => figure.art))
                .Append(story.cardArt)
                .Where(file => file != null).Distinct();
            foreach (var file in files) Assert.IsNotNull(Resources.Load<Texture2D>("Art/" + file), "Resources/Art/" + file + " is missing");
        }

        [TestCaseSource(nameof(AllStories))]
        public void Comic_FiguresStayOnThePanel(Story story)
        {
            foreach (var figure in story.comic.SelectMany(panel => panel.figures))
            {
                Assert.That(figure.x, Is.InRange(0f, 1f), "x of " + figure.kind + " " + figure.art);
                Assert.That(figure.y, Is.InRange(0f, 1f), "y of " + figure.kind + " " + figure.art);
            }
        }

        [TestCaseSource(nameof(AllStories))]
        public void Comic_HasAtLeastThreePanelsAndAtMostSix(Story story)
        {
            if (!story.hasComic) Assert.Pass("no comic");

            Assert.That(story.comic.Length, Is.InRange(3, 6), "fewer than 3 doesn't tell a story; more than 6 doesn't fit the page");
        }
    }
}
