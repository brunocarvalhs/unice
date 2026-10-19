using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The intro comic: while the others hide, the Unice's player (who waits with their eyes closed) reads a one-page comic
    /// of the story, one panel after another until the hiding time runs out. Tap or click skips it.
    /// The panels are drawn with the game's own sprites; <see cref="HideComic"/> decides when and how much.
    /// </summary>
    sealed class HideComicView : UiPart
    {
        /// <summary>Seconds a panel takes to pop onto the page.</summary>
        const float PopSeconds = .35f;
        /// <summary>A panel starts at this scale and grows to 1 while it pops in.</summary>
        const float PopStartScale = .7f;
        /// <summary>How far the figures bob up and down, in panel pixels, and how fast (radians per second).</summary>
        const float BobPixels = 3, BobSpeed = 3;
        /// <summary>Every panel is tilted a little, in degrees, alternating sides, like a hand-pasted page.</summary>
        const float TiltDegrees = .8f;
        /// <summary>Sprite widths in panel pixels (the sprites are 52 px wide, the Unice's frames a bit wider).</summary>
        const float KidWidth = 52, UniceWidth = 66;
        /// <summary>Up to this many panels go in a single row; more are split over two rows.</summary>
        const int MaxPanelsInOneRow = 3;

        public readonly VisualElement Root;
        readonly Label title, countdown;
        readonly VisualElement page;
        readonly List<Panel> panels = new List<Panel>();
        readonly List<(VisualElement holder, float phase)> bobbers = new List<(VisualElement, float)>();
        string builtForStory;
        float time;
        bool phaseEntered, seenBeforeThisPhase, markedSeen, skipped;

        sealed class Panel
        {
            public VisualElement Frame;
            public float Age;
            public float Tilt;
        }

        /// <summary>True while the comic covers the screen (the black "heading home" screen stays hidden then).</summary>
        public bool Playing { get; private set; }

        public HideComicView(Ui ui) : base(ui)
        {
            Root = AddElement(ui.safeArea, "comic");
            var header = IgnorePointer(AddElement(Root, "comic-head"));
            title = IgnorePointer(AddLabel(header, "", "comic-title"));
            countdown = IgnorePointer(AddLabel(header, "", "comic-count"));
            page = IgnorePointer(AddElement(Root, "comic-page"));
            IgnorePointer(LocalizedLabel(Root, "comic.skip", "comic-skip"));
            Root.RegisterCallback<PointerDownEvent>(_ => skipped = true);
            Show(Root, false);
        }

        /// <summary>Per frame: starts, advances and ends the comic according to the game state.</summary>
        public void Frame(float dt)
        {
            if (game.phase != Phase.Hide) { phaseEntered = false; markedSeen = false; skipped = false; }
            else if (!phaseEntered)
            {
                phaseEntered = true;
                var story = Stories.ForMap(game.mapId);
                seenBeforeThisPhase = story != null && app.ComicSeen(story.id);
            }

            bool wanted = !skipped && HideComic.IsWanted(game, app.myCharacter, seenBeforeThisPhase);
            if (wanted != Playing)
            {
                Playing = wanted; Show(Root, wanted);
                if (wanted) Begin();
            }
            if (!Playing) return;

            time += dt;
            SetText(countdown, Localization.Get("comic.countdown", Mathf.CeilToInt(game.timeLeft)));
            int shown = HideComic.PanelsShown(panels.Count, game.hideTime, game.timeLeft);
            for (int i = 0; i < panels.Count; i++) AnimatePanel(panels[i], i < shown, dt);
            foreach (var (holder, phase) in bobbers) holder.style.marginTop = -BobPixels * (.5f + .5f * Mathf.Sin(time * BobSpeed + phase));
        }

        /// <summary>The comic starts: builds the page for this map's story the first time, and counts it as seen.</summary>
        void Begin()
        {
            var story = Stories.ForMap(game.mapId);
            if (story == null) return;
            if (builtForStory != story.id) BuildPage(story);
            foreach (var panel in panels) { panel.Age = 0; panel.Frame.style.opacity = 0; }
            time = 0;
            if (!markedSeen) { markedSeen = true; app.MarkComicSeen(story.id); Metrics.Log("comic_play", ("story", story.id), ("round", game.round)); }
        }

        /// <summary>A panel pops in (fade and grow) when its turn comes; it stays on the page after that.</summary>
        static void AnimatePanel(Panel panel, bool visible, float dt)
        {
            if (!visible) return;
            panel.Age += dt;
            float progress = Mathf.Clamp01(panel.Age / PopSeconds);
            float eased = 1 - (1 - progress) * (1 - progress);
            float scale = Mathf.Lerp(PopStartScale, 1, eased);
            panel.Frame.style.opacity = progress;
            panel.Frame.style.scale = new Scale(new Vector3(scale, scale, 1));
            panel.Frame.style.rotate = new Rotate(new Angle(panel.Tilt * eased, AngleUnit.Degree));
        }

        // ---------- the page ----------
        /// <summary>Lays out the story's panels in one row, or two when there are more than three, and draws each one.</summary>
        void BuildPage(Story forStory)
        {
            builtForStory = forStory.id;
            SetText(title, Localization.Get("stories." + forStory.id + ".name"));
            page.Clear(); panels.Clear(); bobbers.Clear();
            int rowSize = forStory.comic.Length > MaxPanelsInOneRow ? (forStory.comic.Length + 1) / 2 : forStory.comic.Length;
            var groundColor = Art.HexColor(Maps.ById(forStory.mapId).groundTint);
            VisualElement row = null;
            for (int i = 0; i < forStory.comic.Length; i++)
            {
                if (i % rowSize == 0) row = IgnorePointer(AddElement(page, "comic-row"));
                var frame = IgnorePointer(AddElement(row, "comic-panel"));
                if (i == forStory.comic.Length - 1) frame.AddToClassList("comic-panel-last");
                var art = IgnorePointer(AddElement(frame, "comic-art"));
                var panel = forStory.comic[i];
                if (panel.backdrop != null) art.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Art/" + panel.backdrop));
                else art.style.backgroundColor = groundColor;
                foreach (var figure in panel.figures) DrawFigure(art, figure);
                IgnorePointer(LocalizedLabel(frame, "comic." + forStory.id + "." + (i + 1), "comic-caption"));
                panels.Add(new Panel { Frame = frame, Tilt = i % 2 == 0 ? -TiltDegrees : TiltDegrees });
            }
        }

        /// <summary>Puts one figure of the story's panel on the picture.</summary>
        void DrawFigure(VisualElement art, ComicFigure figure)
        {
            switch (figure.kind)
            {
                case ComicFigureKind.Kid: AddKid(art, StoryScreen.Cousins[figure.cousin % StoryScreen.Cousins.Length], figure.x, figure.y, figure.flip); break;
                case ComicFigureKind.Unice: AddUnice(art, figure.x, figure.y, figure.flip); break;
                default: AddSprite(art, figure.art, figure.width, figure.x, figure.y, figure.bob); break;
            }
        }

        // ---------- panel helpers ----------
        /// <summary>A box inside the picture whose anchor is its bottom center (its feet), at (x, y) fractions of the panel.</summary>
        VisualElement AddHolder(VisualElement art, float width, float height, float x, float y, bool flip, bool bob)
        {
            var holder = IgnorePointer(AddElement(art, "comic-figure"));
            var style = holder.style;
            style.width = width; style.height = height;
            style.left = Length.Percent(x * 100); style.top = Length.Percent(y * 100);
            style.translate = new Translate(Length.Percent(-50), Length.Percent(-100));
            style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(100));
            if (flip) style.scale = new Scale(new Vector3(-1, 1, 1));
            if (bob) bobbers.Add((holder, x * 7));
            return holder;
        }

        void AddSprite(VisualElement art, string file, float width, float x, float y, bool bob = false)
        {
            var texture = Resources.Load<Texture2D>("Art/" + file);
            var holder = AddHolder(art, width, texture != null ? width * texture.height / texture.width : width, x, y, false, bob);
            holder.style.backgroundImage = new StyleBackground(texture);
        }

        void AddKid(VisualElement art, int[] kit, float x, float y, bool flip = false)
        {
            var portrait = new CharacterPortrait(KidWidth);
            portrait.ShowKit(Kit.Sanitize(kit));
            AddHolder(art, KidWidth, portrait.style.height.value.value, x, y, flip, bob: true).Add(portrait);
        }

        void AddUnice(VisualElement art, float x, float y, bool flip = false)
        {
            var portrait = new CharacterPortrait(UniceWidth);
            portrait.SetUnice();
            AddHolder(art, UniceWidth, portrait.style.height.value.value, x, y, flip, bob: false).Add(portrait);
        }
    }
}
