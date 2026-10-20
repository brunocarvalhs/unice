using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The story of the game as a full-screen animation: five scenes (one per part of the cordel in Localization,
    /// "story.N.t" / "story.N.b") acted out with the game's own sprites, while the verses appear line by line.
    /// Tap or click to show the rest of a stanza and then move on; the scenes also advance by themselves.
    /// </summary>
    sealed class StoryScreen : UiPart
    {
        const int SceneCount = 5, MaxLines = 7;
        /// <summary>Seconds between one verse line and the next, and how long a finished stanza waits before the next one.</summary>
        const float LineSeconds = 1.5f, HoldSeconds = 2.5f;
        const float KidWidth = 96, UniceWidth = 124;
        /// <summary>The Unice and the walking kids use these sprite frames; kids walk on 1..6 (0 = standing).</summary>
        const int KidWalkFrames = 6, UniceFrames = 8;

        static readonly string[] Backdrops = { "floor_kitchen", "floor_master", "floor_living", "floor_dining", "floor_dining" };

        // the cousins of the cordel: skin, hair, hair color, outfit, outfit color, hat, accessory
        internal static readonly int[][] Cousins =
        {
            new[] { 2, 0, 0, 2, 0, 3, 1 },   // Márcio: straw hat and satchel
            new[] { 1, 2, 1, 3, 3, 0, 3 },   // pigtails and a rag doll
            new[] { 3, 1, 3, 1, 1, 4, 0 },   // leather hat
            new[] { 0, 3, 2, 0, 5, 1, 2 },   // cap and glasses
            new[] { 2, 0, 4, 4, 2, 0, 0 },   // cape
        };

        public readonly VisualElement Root;
        readonly Action onDone;
        readonly VisualElement world, actors, backdrop, flash, sepia, dots;
        readonly Label title;
        readonly Label[] lines = new Label[MaxLines];
        readonly VisualElement[] dotElements = new VisualElement[SceneCount];
        readonly Button doneButton;
        readonly Label hint, credit;

        int scene, stanza, linesShown;
        float sceneTime, stanzaTime, frameSeconds;
        string[][] stanzas = new string[0][];
        Action tick;

        /// <summary>A kid or the Unice on the stage; <see cref="Holder"/> is what moves, scales and flips.</summary>
        sealed class Figure
        {
            public VisualElement Holder;
            public CharacterPortrait Portrait;
            public int[] Kit;
            public string pose;
        }

        /// <summary>Builds the stage (backdrop, props, vignette), the verse card, the progress dots and the skip button inside <paramref name="parent"/>.</summary>
        /// <param name="onDone">Called when the story ends or is skipped (the menu goes back to its main page).</param>
        public StoryScreen(Ui ui, VisualElement parent, Action onDone) : base(ui)
        {
            this.onDone = onDone;
            Root = AddElement(parent, "story");
            var stage = AddElement(Root, "story-stage");
            backdrop = AddElement(stage, "story-backdrop");
            world = AddElement(stage, "story-world");
            actors = AddElement(world, "story-world");
            sepia = IgnorePointer(AddElement(stage, "story-sepia"));
            IgnorePointer(AddElement(stage, "story-vignette"));
            flash = IgnorePointer(AddElement(stage, "story-flash"));

            title = IgnorePointer(AddLabel(Root, "", "story-title"));
            dots = IgnorePointer(AddElement(Root, "story-dots"));
            for (int i = 0; i < SceneCount; i++) dotElements[i] = AddElement(dots, "story-dot");

            var cardRow = AddElement(Root, "story-cardrow");
            var card = AddElement(cardRow, "story-card");
            for (int i = 0; i < MaxLines; i++) lines[i] = AddLabel(card, "", "story-line");
            hint = LocalizedLabel(card, "menu.story_tap", "story-hint");
            credit = LocalizedLabel(card, "menu.story_by", "story-hint");
            doneButton = LocalizedButton(card, "menu.story_done", () => this.onDone(), "mbtn", "mbtn-back");

            LocalizedButton(Root, "menu.story_skip", () => this.onDone(), "mbtn", "mexit");
            Root.RegisterCallback<PointerDownEvent>(OnPointerDown);
        }

        // ---------- flow ----------
        /// <summary>Starts over from the first scene (each time the story page opens).</summary>
        public void Begin() => BeginScene(0);

        /// <summary>Tap or click on the stage: first shows the rest of the stanza, then moves to the next one. Buttons keep their own clicks.</summary>
        void OnPointerDown(PointerDownEvent e)
        {
            // the buttons (skip, done) handle their own clicks
            var element = e.target as VisualElement;
            if (element is Button || element?.GetFirstAncestorOfType<Button>() != null) return;
            if (linesShown < stanzas[stanza].Length) { stanzaTime = stanzas[stanza].Length * LineSeconds; return; }
            NextStanza();
        }

        /// <summary>True on the last stanza of the last scene: the story waits there for "Let's play!" instead of advancing.</summary>
        bool IsLast => scene == SceneCount - 1 && stanza == stanzas.Length - 1;

        /// <summary>The next stanza of this scene, or the first one of the next scene; does nothing at the very end.</summary>
        void NextStanza()
        {
            if (stanza + 1 < stanzas.Length) BeginStanza(stanza + 1);
            else if (scene + 1 < SceneCount) BeginScene(scene + 1);
        }

        /// <summary>Rebuilds the stage for scene <paramref name="index"/>: title, progress dots, backdrop, stanzas and the actors with their animation.</summary>
        void BeginScene(int index)
        {
            scene = index; sceneTime = 0; tick = null;
            actors.Clear();
            world.style.scale = new Scale(Vector3.one); actors.style.scale = new Scale(Vector3.one);
            flash.style.opacity = 0; sepia.style.opacity = 0;
            SetText(title, Localization.Get("story." + (index + 1) + ".t"));
            for (int i = 0; i < SceneCount; i++) dotElements[i].EnableInClassList("story-dot-on", i <= index);
            backdrop.style.backgroundImage = new StyleBackground(Resources.Load<Texture2D>("Art/" + Backdrops[index]));
            var parts = Localization.Get("story." + (index + 1) + ".b").Replace("\r", "").Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            stanzas = new string[parts.Length][];
            for (int i = 0; i < parts.Length; i++) stanzas[i] = parts[i].Split('\n');
            switch (index)
            {
                case 0: tick = KitchenScene(); break;
                case 1: tick = AuntsRoomScene(); break;
                case 2: tick = ScrapScene(); break;
                case 3: tick = HidingScene(); break;
                default: tick = LunchScene(); break;
            }
            BeginStanza(0);
            tick?.Invoke();   // place everything before the first frame is drawn
        }

        /// <summary>Starts a stanza: shows only its first line; the others appear as <see cref="stanzaTime"/> grows.</summary>
        void BeginStanza(int index)
        {
            stanza = index; stanzaTime = 0; linesShown = -1;
            for (int i = 0; i < MaxLines; i++) lines[i].style.display = i < stanzas[index].Length ? DisplayStyle.Flex : DisplayStyle.None;
            ShowLines();
        }

        /// <summary>Reveals the lines whose time has come (only writes the labels when the count changes) and swaps the hint for the done button at the end.</summary>
        void ShowLines()
        {
            int count = stanzas[stanza].Length;
            int shown = Mathf.Min(count, 1 + (int)(stanzaTime / LineSeconds));
            if (shown != linesShown)
            {
                linesShown = shown;
                for (int i = 0; i < count; i++) { lines[i].text = stanzas[stanza][i]; lines[i].style.opacity = i < shown ? 1 : 0; }
            }
            bool done = IsLast && shown == count;
            Show(doneButton, done); Show(credit, done); Show(hint, !done);
        }

        /// <summary>Per frame while the story shows.</summary>
        public void Frame(float dt)
        {
            frameSeconds = Mathf.Min(dt, .1f);
            sceneTime += frameSeconds; stanzaTime += frameSeconds;
            ShowLines();
            bool finished = linesShown == stanzas[stanza].Length;
            if (finished && !IsLast && stanzaTime > stanzas[stanza].Length * LineSeconds + HoldSeconds) NextStanza();
            tick?.Invoke();
        }

        // ---------- scenes ----------
        /// <summary>1. Grandma's kitchen: the kids run through the house, steam rises from the table, the sun is high.</summary>
        Action KitchenScene()
        {
            AddSprite("furn_20", 150, .2f, .52f);
            AddSprite("furn_21", 190, .74f, .52f);
            var sun = AddDot(new Color32(242, 182, 50, 255), 130, .88f, .22f);
            var steam = new[] { AddDot(Color.white, 26, 0, 0), AddDot(Color.white, 26, 0, 0), AddDot(Color.white, 26, 0, 0) };
            var kids = new Figure[3];
            for (int i = 0; i < kids.Length; i++) kids[i] = AddKid(Cousins[i]);
            return () =>
            {
                for (int i = 0; i < kids.Length; i++)
                {
                    float x = Mathf.Repeat(sceneTime * .1f + i * .34f, 1.25f) - .125f;
                    Place(kids[i].Holder, x, .62f + i * .025f, lift: Mathf.Abs(Mathf.Sin(sceneTime * 9 + i)) * 5);
                    PoseKid(kids[i], 1 + (int)(sceneTime * 10 + i * 2) % KidWalkFrames);
                }
                Place(sun, .88f, .22f, 1 + .08f * Mathf.Sin(sceneTime * 2)); sun.style.opacity = .7f + .2f * Mathf.Sin(sceneTime * 1.7f);
                for (int i = 0; i < steam.Length; i++)
                {
                    float u = Mathf.Repeat(sceneTime * .5f + i * .33f, 1);
                    Place(steam[i], .73f + .018f * i + .008f * Mathf.Sin(sceneTime * 3 + i), .42f, .4f + u * .8f, lift: u * 60);
                    steam[i].style.opacity = (1 - u) * .6f;
                }
            };
        }

        /// <summary>2. Aunt Unice's room: she watches over the doll while Márcio sneaks in and freezes whenever she looks.</summary>
        Action AuntsRoomScene()
        {
            AddSprite("furn_1", 85, .12f, .56f);
            AddSprite("furn_0", 175, .72f, .6f);
            var glow = AddSprite("ring", 100, .7f, .46f);
            glow.style.unityBackgroundImageTintColor = (Color)new Color32(242, 182, 50, 255);
            var doll = AddSprite("doll", 40, .7f, .46f);
            var marcio = AddKid(Cousins[0]);
            var unice = AddUnice();
            var whisper = AddBubble(Localization.Get("ev.pass.fx"));
            return () =>
            {
                Place(glow, .7f, .46f, 1 + .12f * Mathf.Sin(sceneTime * 3), lift: 6); glow.style.opacity = .5f + .3f * Mathf.Sin(sceneTime * 3);
                Place(doll, .7f, .46f, lift: 8 + 3 * Mathf.Sin(sceneTime * 2.5f));
                bool watching = (int)(sceneTime / 3) % 2 == 1;
                Place(unice.Holder, .5f, .64f, flip: watching); PoseUnice(unice, false, (int)(sceneTime * 2) % UniceFrames);
                float p = Mathf.Repeat(sceneTime, 8) / 8;
                float x = p < .6f ? Mathf.Lerp(-.1f, .36f, p / .6f) : p < .75f ? .36f : Mathf.Lerp(.36f, -.1f, (p - .75f) / .25f);
                bool moving = p < .6f || p > .75f;
                Place(marcio.Holder, x, .66f, flip: p > .75f, lift: moving ? Mathf.Abs(Mathf.Sin(sceneTime * 7)) * 4 : 0);
                PoseKid(marcio, moving ? 1 + (int)(sceneTime * 7) % KidWalkFrames : 0);
                SetBubble(whisper, !moving, x, .66f, KidWidth * 1.4f);
            };
        }

        /// <summary>3. The scrap: the doll hops from kid to kid until Aunt Unice storms in like a thunderstorm.</summary>
        Action ScrapScene()
        {
            AddSprite("furn_14", 230, .5f, .46f);
            float[] xs = { .22f, .42f, .62f };
            var kids = new Figure[3];
            for (int i = 0; i < kids.Length; i++) kids[i] = AddKid(Cousins[i + 1]);
            var doll = AddSprite("doll", 36, .22f, .6f);
            var unice = AddUnice();
            var whisper = AddBubble(Localization.Get("ev.pass.fx"));
            return () =>
            {
                bool storm = stanza >= 1;
                for (int i = 0; i < kids.Length; i++)
                {
                    float shake = storm ? Mathf.Sin(sceneTime * 40 + i) * .004f : 0;
                    Place(kids[i].Holder, xs[i] + shake, .62f, flip: i == 2, lift: storm ? Mathf.Abs(Mathf.Sin(sceneTime * 12 + i)) * 6 : 0);
                    PoseKid(kids[i], 0);
                }
                float leg = sceneTime / 1.1f, u = leg - Mathf.Floor(leg);
                int from = (int)leg % 3, to = (from + 1) % 3;
                Place(doll, Mathf.Lerp(xs[from], xs[to], Smooth(u)), .62f, lift: 70 + Mathf.Sin(Mathf.PI * u) * 55, rotation: u * 360);
                SetBubble(whisper, u < .35f, xs[from], .62f, KidWidth * 1.4f);

                float arrive = storm ? Smooth(stanzaTime / 2.4f) : 0;
                Place(unice.Holder, Mathf.Lerp(1.15f, .88f, arrive), .66f, flip: true);
                PoseUnice(unice, arrive > 0 && arrive < 1, (int)(sceneTime * 8) % UniceFrames);
                flash.style.opacity = storm ? Mathf.Max(0, .8f * (1 - stanzaTime / .5f)) : 0;
            };
        }

        /// <summary>4. Everyone in their own corner: giggles come from behind the wardrobe, the chest and the table while the aunt patrols.</summary>
        Action HidingScene()
        {
            // the hiding kids are added before their furniture so it covers part of them
            var behindWardrobe = AddKid(Cousins[2]); var behindChest = AddKid(Cousins[3]); var behindTable = AddKid(Cousins[4]);
            AddSprite("furn_1", 92, .14f, .56f);
            AddSprite("furn_17", 290, .54f, .58f);
            AddSprite("furn_6", 120, .86f, .6f);
            var unice = AddUnice();
            var giggle = AddBubble(Localization.Get("ev.giggle.fx"));
            var caught = AddBubble(Localization.Get("ev.catch.gotcha"));
            float unicex = .15f;
            var hiders = new[] { (x: .205f, y: .56f, lift: 125f), (x: .86f, y: .6f, lift: 100f), (x: .705f, y: .58f, lift: 125f) };
            return () =>
            {
                Place(behindWardrobe.Holder, .205f + .012f * Mathf.Sin(sceneTime * 1.3f), .56f); PoseKid(behindWardrobe, 0);
                bool chestPop = stanza >= 1 && stanzaTime > 3;
                float pop = chestPop ? Mathf.Abs(Mathf.Sin((stanzaTime - 3) * 6)) * Mathf.Max(0, 1 - (stanzaTime - 3) / 2.5f) * 50 : 0;
                Place(behindChest.Holder, .86f, .6f, lift: 12 + 12 * Mathf.Max(0, Mathf.Sin(sceneTime * 1.7f)) + pop); PoseKid(behindChest, 0);
                Place(behindTable.Holder, .705f + .01f * Mathf.Sin(sceneTime * 1.1f), .58f); PoseKid(behindTable, 0);

                if (stanza == 0) unicex = .15f + .7f * Mathf.PingPong(sceneTime * .08f, 1);
                else unicex = Mathf.MoveTowards(unicex, .74f, frameSeconds * .1f);
                float next = stanza == 0 ? .15f + .7f * Mathf.PingPong(sceneTime * .08f + .01f, 1) : .74f;
                bool moving = Mathf.Abs(next - unicex) > .0001f;
                Place(unice.Holder, unicex, .68f, flip: next < unicex, lift: moving ? Mathf.Abs(Mathf.Sin(sceneTime * 8)) * 3 : 0);
                PoseUnice(unice, moving, (int)(sceneTime * 8) % UniceFrames);

                float u = Mathf.Repeat(sceneTime, 2) / 2;
                var hider = hiders[(int)(sceneTime / 2) % hiders.Length];
                SetBubble(giggle, true, hider.x, hider.y, hider.lift + u * 24); giggle.style.opacity = 1 - u;
                SetBubble(caught, chestPop && stanzaTime < 5.5f, .86f, .6f, 150);
            };
        }

        /// <summary>5. Lunch: the doll is back in the aunt's arms, grandma calls from the kitchen and the scene turns into a photograph.</summary>
        Action LunchScene()
        {
            AddSprite("furn_17", 300, .74f, .58f);
            var unice = AddUnice();
            var doll = AddSprite("doll", 54, .4f, .6f);
            var hearts = new[] { AddDot(new Color32(239, 107, 111, 255), 18, 0, 0), AddDot(new Color32(239, 107, 111, 255), 18, 0, 0), AddDot(new Color32(239, 107, 111, 255), 18, 0, 0) };
            var kids = new Figure[4];
            for (int i = 0; i < kids.Length; i++) kids[i] = AddKid(Cousins[i]);
            var grandma = AddBubble(Localization.Get("story.5.t"));
            float[] seats = { .6f, .68f, .8f, .88f };
            return () =>
            {
                Place(unice.Holder, .34f, .64f); PoseUnice(unice, false, (int)(sceneTime * 2) % UniceFrames);
                Place(doll, .385f, .64f, lift: 38 + 3 * Mathf.Sin(sceneTime * 3));
                for (int i = 0; i < hearts.Length; i++)
                {
                    float u = Mathf.Repeat(sceneTime * .4f + i * .33f, 1);
                    Place(hearts[i], .34f + .03f * Mathf.Sin(sceneTime * 2 + i * 2), .64f, .6f + u * .6f, lift: 130 + u * 70);
                    hearts[i].style.opacity = 1 - u;
                }
                for (int i = 0; i < kids.Length; i++)
                {
                    float start = .52f - i * .09f - .1f, goal = seats[i];
                    float run = stanza >= 1 ? Mathf.Clamp01((stanzaTime - i * .5f) / 3) : 0;
                    bool moving = run > 0 && run < 1;
                    float x = stanza >= 1 ? Mathf.Lerp(start, goal, run) : start + .02f * Mathf.Sin(sceneTime * 1.5f + i);
                    Place(kids[i].Holder, x, .62f + (i % 2) * .03f, lift: moving ? Mathf.Abs(Mathf.Sin(sceneTime * 10 + i)) * 5 : 0);
                    PoseKid(kids[i], moving ? 1 + (int)(sceneTime * 10 + i * 2) % KidWalkFrames : 0);
                }
                SetBubble(grandma, sceneTime > 1.5f, .86f, .3f, 0, 1 + .05f * Mathf.Sin(sceneTime * 4));
                float photo = stanza >= 2 ? Smooth(stanzaTime / 3) : 0;
                sepia.style.opacity = photo * .45f;
                world.style.scale = new Scale(Vector3.one * (1 + .06f * photo));
            };
        }

        // ---------- stage helpers ----------
        /// <summary>Ease in and out: 0..1 in, 0..1 out (smoothstep).</summary>
        static float Smooth(float t) { t = Mathf.Clamp01(t); return t * t * (3 - 2 * t); }

        /// <summary>An empty, absolutely positioned box on the stage whose anchor is its bottom center (its "feet"); <see cref="Place"/> moves it.</summary>
        VisualElement NewHolder(float width, float height)
        {
            var holder = AddElement(actors, "story-prop");
            var style = holder.style;
            style.width = width; style.height = height;
            style.translate = new Translate(Length.Percent(-50), Length.Percent(-100));
            style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(100));
            return holder;
        }

        /// <summary>Puts a holder at (x, y), fractions of the stage where y is the feet line; <paramref name="lift"/> raises it in panel pixels (jumps, bobbing).</summary>
        /// <param name="flip">Mirrored (facing left).</param>
        /// <param name="rotation">Degrees, around the feet.</param>
        static void Place(VisualElement holder, float x, float y, float scale = 1, bool flip = false, float lift = 0, float rotation = 0)
        {
            var style = holder.style;
            style.left = Length.Percent(x * 100); style.top = Length.Percent(y * 100); style.marginTop = -lift;
            style.scale = new Scale(new Vector3(flip ? -scale : scale, scale, 1));
            style.rotate = new Rotate(new Angle(rotation, AngleUnit.Degree));
        }

        /// <summary>A sprite from Resources/Art (a piece of furniture, the doll…) at the given width in panel pixels; the height follows the image.</summary>
        VisualElement AddSprite(string file, float width, float x, float y)
        {
            var texture = Resources.Load<Texture2D>("Art/" + file);
            var holder = NewHolder(width, texture != null ? width * texture.height / texture.width : width);
            holder.style.backgroundImage = new StyleBackground(texture);
            Place(holder, x, y);
            return holder;
        }

        /// <summary>A round, tinted dot (sun, steam, hearts).</summary>
        VisualElement AddDot(Color tint, float size, float x, float y)
        {
            var dot = AddSprite("dot", size, x, y);
            dot.style.unityBackgroundImageTintColor = tint;
            return dot;
        }

        /// <summary>A kid drawn with the same layered sprites as in the game, from a fixed look.</summary>
        Figure AddKid(int[] kit)
        {
            var figure = new Figure { Kit = Kit.Sanitize(kit), Portrait = new CharacterPortrait(KidWidth) };
            figure.Holder = NewHolder(KidWidth, KidWidth * 68 / 52);
            figure.Holder.Add(figure.Portrait);
            return figure;
        }

        /// <summary>Aunt Unice, one sprite per pose (idle or walking).</summary>
        Figure AddUnice()
        {
            var figure = new Figure { Portrait = new CharacterPortrait(UniceWidth) };
            figure.Holder = NewHolder(UniceWidth, UniceWidth * 68 / 52);
            figure.Holder.Add(figure.Portrait);
            return figure;
        }

        /// <summary>Sets the walk frame (0 = standing); only touches the sprites when it changes (walking is 10 frames a second, not 60).</summary>
        static void PoseKid(Figure kid, int frame)
        {
            string key = "k" + frame;
            if (kid.pose == key) return;
            kid.pose = key; kid.Portrait.ShowKit(kid.Kit, false, frame);
        }

        /// <summary>Walking (<c>unice_m</c>) or standing (<c>unice_i</c>) frame; like <see cref="PoseKid"/>, only touches the sprite when the pose changes.</summary>
        static void PoseUnice(Figure unice, bool moving, int frame)
        {
            string file = "unice_" + (moving ? "m" : "i") + "_s_" + frame;
            if (unice.pose == file) return;
            unice.pose = file; unice.Portrait.SetUnice(file);
        }

        /// <summary>A hidden speech bubble; <see cref="SetBubble"/> shows and places it.</summary>
        Label AddBubble(string text)
        {
            var bubble = AddLabel(actors, text, "story-bubble");
            bubble.pickingMode = PickingMode.Ignore;
            bubble.style.translate = new Translate(Length.Percent(-50), Length.Percent(-100));
            bubble.style.display = DisplayStyle.None;
            return bubble;
        }

        /// <summary>Shows a bubble at (x, y) of the stage, <paramref name="height"/> panel pixels above the feet, or hides it.</summary>
        static void SetBubble(Label bubble, bool visible, float x, float y, float height, float scale = 1)
        {
            Show(bubble, visible);
            if (!visible) return;
            bubble.style.left = Length.Percent(x * 100); bubble.style.top = Length.Percent(y * 100); bubble.style.marginTop = -height;
            bubble.style.scale = new Scale(new Vector3(scale, scale, 1));
        }
    }
}
