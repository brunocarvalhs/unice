using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>The main menu over the house map: the main page and its sub-pages (online, how to play, my child, credits).</summary>
    sealed class HomeMenu : UiPart
    {
        enum Page { Main, Online, HowToPlay, MyKid, Credits, Stories }

        /// <summary>A typed room code needs at least this many characters.</summary>
        const int MinRoomCodeLength = 4;

        public readonly VisualElement Root;
        readonly VisualElement mainPage, onlinePage, myKidPage, storiesPage, kidCustomizerSlot;
        VisualElement howToPlayPage, creditsPage;   // set by BuildHowToPlay
        CreditsPage credits;   // set by BuildCredits
        readonly TextField roomCodeField;
        readonly Label netStatusLabel;
        readonly Button createRoomButton, joinRoomButton, soloButton, onlineButton;
        Page currentPage = Page.Main;
        /// <summary>What the story picker does once a story is chosen (start solo, create the room), and the page to show meanwhile.</summary>
        Action startWithChosenStory;
        Page pageAfterPick = Page.Main;

        public HomeMenu(Ui ui) : base(ui)
        {
            // main page: laid out in Resources/UI/Screens/HomeMenu.uxml (UI Builder); here only the texts and clicks
            Root = ui.LoadScreen("HomeMenu", ui.root);
            Root.Q<Label>("version").text = "v" + Application.version;
            mainPage = Root.Q("main-page");
            ui.Localize(Root.Q<Label>("sub"), "menu.sub");
            soloButton = Root.Q<Button>("local"); onlineButton = Root.Q<Button>("online");
            ui.Localize(soloButton, "menu.local").clicked += () => ChooseStory(app.PlaySolo, Page.Main);
            ui.Localize(onlineButton, "menu.online").clicked += () => { app.SetNetStatus(""); ShowPage(Page.Online); };
            ui.Localize(Root.Q<Button>("howto"), "menu.howto").clicked += () => ShowPage(Page.HowToPlay);
            ui.Localize(Root.Q<Button>("mykid"), "menu.mykid").clicked += () => ShowPage(Page.MyKid);
            ui.Localize(Root.Q<Button>("credits"), "menu.credits").clicked += () => ShowPage(Page.Credits);

            // online: create a room (the code is generated for you) or join one with a code
            onlinePage = Root.Q("online-page");
            ui.Localize(onlinePage.Q<Label>("title"), "menu.online_title");
            ui.Localize(onlinePage.Q<Label>("desc"), "menu.online_desc");
            createRoomButton = ui.Localize(onlinePage.Q<Button>("create"), "menu.create_room");
            createRoomButton.clicked += () => ChooseStory(() => app.EnterRoom(true, null), Page.Online);
            ui.Localize(onlinePage.Q<Label>("or"), "menu.or_join");
            roomCodeField = onlinePage.Q<TextField>("room-code");
            // codes are A-Z/0-9: show them that way while typing
            roomCodeField.RegisterValueChangedCallback(e =>
            {
                var clean = TextFormat.CleanCode(e.newValue);
                if (clean != e.newValue) roomCodeField.SetValueWithoutNotify(clean);
            });
            joinRoomButton = ui.Localize(onlinePage.Q<Button>("join"), "menu.join");
            joinRoomButton.clicked += JoinTypedRoom;
            netStatusLabel = onlinePage.Q<Label>("net");
            BackToMain(onlinePage, "menu.back");

            // story picker: solo and "create room" come here first; the story decides the map
            storiesPage = Root.Q("stories-page");
            var stories = storiesPage.Q("panel");
            ui.Localize(stories.Q<Label>("title"), "menu.stories_title");
            ui.Localize(stories.Q<Label>("desc"), "menu.stories_desc");
            new StoryPicker(ui, stories, PickStory, () => ShowPage(Page.Main));

            BuildHowToPlay();

            creditsPage = Root.Q("credits-page");
            ui.Localize(creditsPage.Q<Label>("title"), "menu.credits");
            credits = new CreditsPage(ui, creditsPage.Q("panel"));
            BackToMain(creditsPage, "menu.back").BringToFront();   // the credits were added after the button: it goes last

            // my child: the same customization as in the waiting room
            myKidPage = Root.Q("mykid-page");
            ui.Localize(myKidPage.Q<Label>("title"), "menu.mykid_title");
            kidCustomizerSlot = myKidPage.Q("kid-slot");
            BackToMain(myKidPage, "menu.ready");

            // top right: sound and language next to quit; the voice toggles stay at the bottom
            var topRight = Root.Q("top-right");
            ui.settings.AddSoundIconButton(topRight, "mpill"); ui.settings.AddLanguageIconButton(topRight, "mpill");
#if !UNITY_WEBGL
            LocalizedButton(topRight, "menu.quit", () => Application.Quit(), "mbtn", "mbtn-quit");   // a browser tab can't quit itself
#endif
            var footer = Root.Q("footer");
            ui.settings.AddMicButton(footer, "mpill"); ui.settings.AddWhisperButton(footer, "mpill");
            ShowPage(Page.Main);
        }

        /// <summary>Opens the story picker; <paramref name="start"/> runs once a story is chosen, with <paramref name="pageWhileStarting"/> showing meanwhile (where a connection error can be read).</summary>
        void ChooseStory(Action start, Page pageWhileStarting)
        {
            startWithChosenStory = start; pageAfterPick = pageWhileStarting;
            ShowPage(Page.Stories);
        }

        /// <summary>The player chose a story: its map is set and the pending start (solo or room) goes ahead.</summary>
        void PickStory(Story story)
        {
            game.mapId = story.mapId;
            Metrics.Log("story_pick", ("story", story.id), ("map", story.mapId));
            ShowPage(pageAfterPick);
            startWithChosenStory?.Invoke();
        }

        void BuildHowToPlay()
        {
            howToPlayPage = Root.Q("howto-page");
            ui.Localize(howToPlayPage.Q<Label>("title"), "menu.howto_title");
            var scroll = howToPlayPage.Q<ScrollView>("rules");
            void Rule(string titleKey, string bodyKey, string titleClass)
            {
                var rule = AddElement(scroll, "mrule");
                var title = LocalizedLabel(rule, titleKey, "mrule-t");
                if (titleClass != null) title.AddToClassList(titleClass);
                LocalizedLabel(rule, bodyKey, "mnote");
            }
            Rule("rule.unice.t", "rule.unice.b", "mrule-rose");
            Rule("rule.teamwin.t", "rule.teamwin.b", "mrule-moss");
            Rule("rule.unicewin.t", "rule.unicewin.b", "mrule-rose");
            Rule("rule.hidetime.t", "rule.hidetime.b", null);
            Rule("rule.dark.t", "rule.dark.b", null);
            Rule("rule.gigglerule.t", "rule.gigglerule.b", "mrule-rose");
            Rule("rule.laugh.t", "rule.laugh.b", "mrule-rose");
            Rule("rule.rotation.t", "rule.rotation.b", null);
            Rule("rule.controls.t", "rule.controls.b", null);
            BackToMain(howToPlayPage, "menu.back");
        }

        /// <summary>The page's back button (UXML name "back"), with its text key, going to the main page.</summary>
        Button BackToMain(VisualElement page, string textKey)
        {
            var button = ui.Localize(page.Q<Button>("back"), textKey);
            button.clicked += () => ShowPage(Page.Main);
            return button;
        }

        /// <summary>Each time home appears: back to the main page, or to "online" if there's a connection error to read.</summary>
        public void OnShow()
        {
            currentPage = Page.Main;   // the phase's screen_view ("home") already counted the main page
            ShowPage(app.netStatusIsError && app.netStatusText.Length > 0 ? Page.Online : Page.Main);
        }

        /// <summary>Per step: the buttons follow the connection state.</summary>
        public void Refresh()
        {
            bool canGoOnline = app.NetAvailable && !app.isBusy;
            createRoomButton.SetEnabled(canGoOnline); joinRoomButton.SetEnabled(canGoOnline);
            soloButton.SetEnabled(!app.isBusy); onlineButton.SetEnabled(!app.isBusy);
            bool hasStatus = app.netStatusText.Length > 0;
            SetText(netStatusLabel, hasStatus ? app.netStatusText : Localization.Get(app.NetAvailable ? "menu.net_ready" : "menu.net_unavailable"));
            netStatusLabel.EnableInClassList("net-err", app.netStatusIsError && hasStatus);
        }

        /// <summary>Per frame while showing.</summary>
        public void Frame()
        {
            if (currentPage == Page.MyKid) ui.kidCustomizer.AnimatePreview();
            if (currentPage == Page.Credits) credits.Frame(Time.unscaledDeltaTime);
        }

        /// <summary>Analytics screen name of each page; the main page is the phase's own "home".</summary>
        static string ScreenName(Page page) => page switch
        {
            Page.Online => "home_online", Page.HowToPlay => "home_how_to_play", Page.MyKid => "home_my_kid", Page.Credits => "home_credits", Page.Stories => "home_stories", _ => "home",
        };

        void ShowPage(Page page)
        {
            if (page != currentPage) Metrics.Screen(ScreenName(page));
            currentPage = page;
            Show(mainPage, page == Page.Main); Show(onlinePage, page == Page.Online);
            Show(howToPlayPage, page == Page.HowToPlay); Show(myKidPage, page == Page.MyKid); Show(storiesPage, page == Page.Stories); Show(creditsPage, page == Page.Credits);
            ui.UpdateBackdrop();
            if (page == Page.Credits) credits.Begin();
            if (page == Page.MyKid) ui.kidCustomizer.ShowIn(kidCustomizerSlot);
        }

        /// <summary>Back button on the menu: sub-pages go back to the main page. False when already there.</summary>
        public bool Back()
        {
            if (ui.currentScreen != UiScreen.Home || currentPage == Page.Main) return false;
            ShowPage(Page.Main);
            return true;
        }

        void JoinTypedRoom()
        {
            var code = TextFormat.CleanCode(roomCodeField.value);
            if (code.Length < MinRoomCodeLength) { app.SetNetStatus(Localization.Get("menu.need_code"), true); return; }
            app.EnterRoom(false, code);
        }
    }
}
