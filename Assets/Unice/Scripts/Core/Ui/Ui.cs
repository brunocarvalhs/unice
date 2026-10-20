using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>Which full screen is showing; follows <see cref="Game.phase"/>.</summary>
    enum UiScreen { None, Home, Lobby, Game, Result, Final }

    /// <summary>
    /// Every screen and the in-game HUD, built in code with UI Toolkit (styles in Resources/UI/unice.uss).
    /// Same screens and texts as the web version, plus touch controls.
    /// This class only assembles the parts, switches screens and owns what several screens share
    /// (localized text, the setting buttons); each screen and overlay lives in its own class.
    /// </summary>
    public sealed class Ui
    {
        public static readonly Color Rose = new Color32(239, 115, 148, 255), Candle = new Color32(240, 176, 87, 255), Muted = new Color32(173, 154, 140, 255),
            Blood = new Color32(226, 74, 82, 255), Moss = new Color32(143, 203, 156, 255), Ink = new Color32(241, 231, 216, 255);

        const float HudRefreshSeconds = .2f;
        /// <summary>Width / height from which the panel counts as <c>wide</c> (20:9 phones, ultrawide monitors) and up to which as <c>boxy</c> (tablets, 4:3).</summary>
        const float WideAspect = 1.9f, BoxyAspect = 1.45f;
        /// <summary>Height in inches of a phone held sideways, the size the 960x540 layout is drawn for; taller screens (tablets, TVs) get proportionally more panel units, up to <see cref="MaxPhysicalScale"/>, so the buttons keep a hand's size instead of growing with the screen.</summary>
        const float PhoneHeightInches = 3f, MaxPhysicalScale = 2f;
        /// <summary>Padding of <c>.overlay</c> in unice.uss (panel pixels), kept around the pause sheet on top of the safe-area insets.</summary>
        const float OverlayPadding = 16;

        /// <summary>
        /// Panel size in USS pixels, matched by height: the panel is always 540 px tall and as wide as the screen's aspect gives
        /// (960 at 16:9, 1200 at 20:9, 720 at 4:3). 540 (not 720) makes 1 px ≈ 0.79 dp on a 1080p ~400 ppi phone, so a 64 px
        /// button is ≥ 48 dp.
        /// </summary>
        public static readonly Vector2Int ReferenceResolution = new Vector2Int(960, 540);

        internal readonly App app;
        Game game => app.game;
        internal readonly VisualElement root;
        /// <summary>The part of the screen not covered by notches and system bars; HUD and messages live inside it.</summary>
        internal readonly VisualElement safeArea;
        /// <summary>Touch input (on-screen controls) or keyboard and mouse (key hints); follows the last device used, see <see cref="InputMode"/>.</summary>
        /// <summary>A TV (Android without a touch screen) is the one place that starts without touch although the platform is mobile.</summary>
        static bool IsTelevision => Application.platform == RuntimePlatform.Android && Touchscreen.current == null;

        internal bool isTouch = Application.isMobilePlatform && !IsTelevision;
        readonly InputMode inputMode;
        internal UiScreen currentScreen = UiScreen.None;
        float hudRefreshTimer;
        Rect lastSafeRect;
        string aspectClass;
        readonly PanelSettings panelSettings;
        int scaledForHeight; float scaledForDpi;

        internal readonly Messages messages;
        /// <summary>The intro comic on the Unice's screen while the others hide; <see cref="Messages"/> hides its black screen while it plays.</summary>
        internal readonly HideComicView comic;
        internal readonly KidCustomizer kidCustomizer;
        internal readonly SettingToggles settings;
        readonly ScreenEffects screenEffects;
        readonly WorldLabels worldLabels;
        readonly SoundCues soundCues;
        readonly Hud hud;
        readonly TouchControls controls;
        /// <summary>The cordel poster behind the main menu and the waiting room (not the intro story, not a match).</summary>
        readonly CordelBackdrop backdrop;
        readonly HomeMenu homeMenu;
        readonly LobbyScreen lobbyScreen;
        readonly ResultScreen resultScreen;
        readonly PauseMenu pauseMenu;
        readonly KeyboardLift homeKeyboardLift, lobbyKeyboardLift;

        public Ui(App app)
        {
            this.app = app;
            inputMode = new InputMode(isTouch);
            // The panel asset made by Unice > Configurar projeto keeps the UI shaders in the build; the fallback is for the editor.
            panelSettings = Resources.Load<PanelSettings>("UI/UnicePanel");
            if (panelSettings == null) panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize; panelSettings.referenceResolution = ReferenceResolution;
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight; panelSettings.match = 1;
            panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("UI/UniceTheme");
            var uiObject = new GameObject("UI"); uiObject.SetActive(false); uiObject.transform.SetParent(app.transform, false);
            var document = uiObject.AddComponent<UIDocument>(); document.panelSettings = panelSettings; uiObject.SetActive(true);
            root = document.rootVisualElement;
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI/unice"));
            root.AddToClassList("root"); root.pickingMode = PickingMode.Ignore;

            // creation order = drawing order (later parts are drawn on top)
            screenEffects = new ScreenEffects(this);
            worldLabels = new WorldLabels(this);
            soundCues = new SoundCues(this);
            safeArea = IgnorePointer(AddElement(root, "layer"));
            hud = new Hud(this);
            controls = new TouchControls(this);
            comic = new HideComicView(this);   // before Messages: banners and toasts stay on top of the comic
            messages = new Messages(this);
            settings = new SettingToggles(this, Relocalize);
            kidCustomizer = new KidCustomizer(this);
            backdrop = new CordelBackdrop(this, root);   // under every screen, and outside their safe-area insets: it bleeds to the screen edges
            homeMenu = new HomeMenu(this); lobbyScreen = new LobbyScreen(this); resultScreen = new ResultScreen(this); pauseMenu = new PauseMenu(this);
            // the screens with text fields: room code and name (My child) at home, name (Appearance tab) in the waiting room
            homeKeyboardLift = new KeyboardLift(this, homeMenu.Root); lobbyKeyboardLift = new KeyboardLift(this, lobbyScreen.Root);
            ApplyInputMode();
            settings.Refresh();
        }

        // ---------- what App and the gameplay code call ----------
        /// <summary>A text field has focus: keyboard keys type instead of controlling the game.</summary>
        public bool Typing
        {
            get
            {
                var focused = root.panel?.focusController?.focusedElement as VisualElement;
                return focused != null && (focused is TextField || focused.GetFirstAncestorOfType<TextField>() != null);
            }
        }
        public Vector2 JoystickDirection => controls.JoystickDirection;
        public bool SneakToggled => controls.SneakToggled;
        public void ReleaseTouchControls() => controls.Release();
        /// <summary>Back button on the menu: returns false when already on the main page.</summary>
        public bool MenuBack() => homeMenu.Back();
        public void ShowPause(bool visible, bool online) => pauseMenu.Show(visible, online);
        public void Banner(string text, BannerStyle style, float seconds) => messages.Banner(text, style, seconds);
        public void Toast(string text) => messages.Toast(text);
        public void Caption(string text) => messages.Caption(text);
        public void Flash(Color color, float seconds) => screenEffects.Flash(color, seconds);
        public void ShowSoundCue(float angle, float loudness, Color color) => soundCues.ShowSoundCue(angle, loudness, color);
        public void ShowWorldText(float x, float y, string text, Color color, float size, float lifetime, bool alwaysVisible = false) =>
            worldLabels.ShowWorldText(x, y, text, color, size, lifetime, alwaysVisible);

        // ---------- localized text built once (menus, rules, static labels) ----------
        // Most text is re-applied every step from the Render/Refresh methods via Localization.Get(...) and follows a language
        // change automatically. These two are for text built ONCE: they remember the key so a language switch
        // can re-apply it later via Relocalize().
        internal readonly List<Action> relocalizers = new List<Action>();
        internal Label LocalizedLabel(VisualElement parent, string key, params string[] classes)
        {
            var label = AddLabel(parent, Localization.Get(key), classes);
            relocalizers.Add(() => label.text = Localization.Get(key));
            return label;
        }
        internal Button LocalizedButton(VisualElement parent, string key, Action onClick, params string[] classes)
        {
            var button = AddButton(parent, Localization.Get(key), onClick, classes);
            relocalizers.Add(() => button.text = Localization.Get(key));
            return button;
        }
        /// <summary>
        /// Builds a screen from its UXML (Resources/UI/Screens, editable in the UI Builder) under <paramref name="parent"/> and returns the file's root element.
        /// The code then finds the elements by name (<c>Q</c>) and gives them their text keys (<see cref="Localize"/>) and clicks.
        /// </summary>
        internal VisualElement LoadScreen(string name, VisualElement parent)
        {
            var asset = Resources.Load<VisualTreeAsset>("UI/Screens/" + name);
            var screen = asset.CloneTree().Q("root");
            screen.styleSheets.Clear();   // the file links unice.uss so the UI Builder can preview it; the panel already has it
            parent.Add(screen);
            return screen;
        }

        /// <summary>A UXML label or button whose text follows language changes: the UXML text is only a preview for the UI Builder.</summary>
        internal T Localize<T>(T element, string key) where T : TextElement
        {
            element.text = Localization.Get(key);
            relocalizers.Add(() => element.text = Localization.Get(key));
            return element;
        }

        void Relocalize()
        {
            foreach (var relocalize in relocalizers) relocalize();
            settings.Refresh(); lobbyScreen.Invalidate(); hud.Invalidate();
        }

        /// <summary>The look changed (name/kit): the waiting room shows it.</summary>
        internal void OnKitChanged() => lobbyScreen.Invalidate();

        // ---------- per step (20/s): screens, HUD, prompt ----------
        public void Step(float dt)
        {
            SyncScreens();
            hudRefreshTimer -= dt;
            if (hudRefreshTimer <= 0 && currentScreen == UiScreen.Game) { hudRefreshTimer = HudRefreshSeconds; hud.Tick(); }
            var hints = ActionHints.For(game, app.myCharacter, showKeys: !isTouch, gamepad: inputMode.Gamepad);
            messages.SetPrompt(hints.text);
            if (isTouch) controls.ShowAvailable(hints);
        }

        static UiScreen ScreenFor(Phase phase) => phase switch
        {
            Phase.Home => UiScreen.Home,
            Phase.Lobby => UiScreen.Lobby,
            Phase.Result => UiScreen.Result,
            Phase.Final => UiScreen.Final,
            _ => UiScreen.Game,
        };

        /// <summary>Shows the screen for the current phase (running each screen's on-show step once), then refreshes it.</summary>
        void SyncScreens()
        {
            var wanted = ScreenFor(game.phase);
            bool resultLike = wanted == UiScreen.Result || wanted == UiScreen.Final;
            if (wanted != currentScreen)
            {
                var previous = currentScreen; currentScreen = wanted;
                Show(homeMenu.Root, wanted == UiScreen.Home); Show(lobbyScreen.Root, wanted == UiScreen.Lobby); Show(resultScreen.Root, resultLike);
                Show(hud.Root, wanted == UiScreen.Game); Show(controls.Root, wanted == UiScreen.Game && isTouch);
                UpdateBackdrop();
                if (wanted == UiScreen.Home) homeMenu.OnShow();
                if (wanted == UiScreen.Lobby) lobbyScreen.OnShow(cameFromFinal: previous == UiScreen.Final);
                if (resultLike) resultScreen.Render(wanted == UiScreen.Final);
                if (wanted == UiScreen.Game) hudRefreshTimer = 0;
                // any screen change closes the pause menu (it can be open on the result screen when the next round starts)
                pauseMenu.Show(false, false); app.paused = false; controls.Release();
                hud.Invalidate();
            }
            if (wanted == UiScreen.Home) homeMenu.Refresh();
            if (wanted == UiScreen.Lobby) lobbyScreen.Render();
            if (resultLike) resultScreen.Refresh(wanted == UiScreen.Final);
        }

        // ---------- per frame: world text, cues, overlays ----------
        public void Frame(float dt, Camera camera)
        {
            if (root.panel == null) return;
            if (inputMode.Update()) ApplyInputMode();
            if (inputMode.Gamepad) KeepPadFocus();
            ApplySafeArea(); homeKeyboardLift.Update(); lobbyKeyboardLift.Update();
            float panelHeight = root.layout.height;
            if (float.IsNaN(panelHeight) || panelHeight < 1) return;   // no layout yet
            ApplyAspectClass(root.layout.width / panelHeight);
            ApplyPhysicalScale();
            messages.Frame(dt);
            worldLabels.Frame(dt, camera);
            soundCues.Frame(dt, camera);
            screenEffects.Frame(dt);
            comic.Frame(dt);
            if (currentScreen == UiScreen.Home) homeMenu.Frame();
            if (backdrop.Root.style.display != DisplayStyle.None) backdrop.Frame();
        }

        /// <summary>Shows the backdrop on every screen but the match itself (and the intro story).</summary>
        internal void UpdateBackdrop()
        {
            if (homeMenu == null) return;   // the menu's own constructor shows its first page before it is assigned
            Show(backdrop.Root, currentScreen == UiScreen.Lobby || currentScreen == UiScreen.Result || currentScreen == UiScreen.Final || currentScreen == UiScreen.Home);
        }

        /// <summary>
        /// With a gamepad the menus are walked by focus (the d-pad and stick move it, A presses, B goes back), so one visible button
        /// must always hold it; when a screen changes and the focused button is gone, the first visible one takes it.
        /// </summary>
        void KeepPadFocus()
        {
            if (currentScreen == UiScreen.Game && !app.paused) return;   // playing: the buttons are not in the way
            var focused = root.panel?.focusController?.focusedElement as VisualElement;
            if (focused != null && focused.resolvedStyle.display != DisplayStyle.None && focused.resolvedStyle.visibility == Visibility.Visible && focused.worldBound.width > 0) return;
            if (Typing) return;
            var first = root.Query<Button>().Where(button => button.resolvedStyle.display != DisplayStyle.None && button.worldBound.width > 0 && button.enabledInHierarchy && button.focusable).First();
            first?.Focus();
        }

        /// <summary>Pretends the player uses touch or keyboard/mouse (layout screenshots).</summary>
        internal void ForceTouch(bool touch) { inputMode.Force(touch); ApplyInputMode(); }

        /// <summary>On phones and tablets (and TVs, which run Android) the panel gets more units on bigger screens: its size follows the screen's physical height, not just its pixels.</summary>
        void ApplyPhysicalScale()
        {
            if (!Application.isMobilePlatform && Application.platform != RuntimePlatform.Android) return;
            if (Screen.height == scaledForHeight && Screen.dpi == scaledForDpi) return;
            scaledForHeight = Screen.height; scaledForDpi = Screen.dpi;
            // a TV reports a made-up dpi and is watched from afar: it always gets the largest scale, so its buttons stay modest
            float inches = Screen.dpi > 1 ? Screen.height / Screen.dpi : PhoneHeightInches;
            float scale = IsTelevision ? MaxPhysicalScale : Mathf.Clamp(inches / PhoneHeightInches, 1, MaxPhysicalScale);
            panelSettings.referenceResolution = new Vector2Int(Mathf.RoundToInt(ReferenceResolution.x * scale), Mathf.RoundToInt(ReferenceResolution.y * scale));
        }

        /// <summary>
        /// Layout classes on the root for unice.uss: <c>touch</c> or <c>pointer</c> (the input in use) and <c>wide</c>, <c>normal</c> or <c>boxy</c> (the aspect).
        /// A rule written under one of them only applies to that kind of device.
        /// </summary>
        void ApplyAspectClass(float aspect)
        {
            string kind = aspect >= WideAspect ? "wide" : aspect <= BoxyAspect ? "boxy" : "normal";
            if (kind == aspectClass) return;
            if (aspectClass != null) root.RemoveFromClassList(aspectClass);
            aspectClass = kind; root.AddToClassList(kind);
        }

        /// <summary>The player switched between touch and keyboard/mouse: shows the matching controls and spacing.</summary>
        void ApplyInputMode()
        {
            isTouch = inputMode.Touch;
            root.EnableInClassList("touch", isTouch); root.EnableInClassList("pointer", !isTouch);
            root.EnableInClassList("pad", inputMode.Gamepad);
            hud.SetDocked(!isTouch);
            Show(controls.Root, currentScreen == UiScreen.Game && isTouch);
            messages.SetTouchLayout(isTouch);
            if (!isTouch) controls.Release();   // a held joystick or the sneak toggle must not keep driving the character
        }

        /// <summary>Keeps the HUD and the full screens inside Screen.safeArea (notches, rounded corners, system bars).</summary>
        void ApplySafeArea()
        {
            var safeRect = Screen.safeArea;
            if (safeRect == lastSafeRect || Screen.height < 1) return;
            lastSafeRect = safeRect;
            float pixelsToPanel = root.layout.height > 0 ? root.layout.height / Screen.height : (float)ReferenceResolution.y / Screen.height;
            float left = safeRect.xMin * pixelsToPanel, right = (Screen.width - safeRect.xMax) * pixelsToPanel;
            float top = (Screen.height - safeRect.yMax) * pixelsToPanel, bottom = safeRect.yMin * pixelsToPanel;
            foreach (var screen in new[] { safeArea, homeMenu.Root, lobbyScreen.Root, resultScreen.Root })
            {
                screen.style.left = left; screen.style.right = right; screen.style.top = top; screen.style.bottom = bottom;
            }
            // the pause dims the whole screen (the game draws under the notch too) but keeps its sheet inside the safe area
            var pause = pauseMenu.Root.style;
            pause.paddingLeft = OverlayPadding + left; pause.paddingRight = OverlayPadding + right;
            pause.paddingTop = OverlayPadding + top; pause.paddingBottom = OverlayPadding + bottom;
        }
    }
}
