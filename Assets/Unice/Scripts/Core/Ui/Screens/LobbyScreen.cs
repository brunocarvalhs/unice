using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The waiting room (full screen, made for a phone on its side): the room code, the players, and a side panel with
    /// two tabs — match options and the player's appearance — plus the ready / start button.
    /// </summary>
    sealed class LobbyScreen : UiPart
    {
        enum SideTab { Match, Appearance }

        const int CopiedFeedbackMs = 1800;

        public readonly VisualElement Root;
        readonly VisualElement playerCards, matchTab, appearanceTab, matchTabButton, appearanceTabButton;
        readonly Label titleLabel, codeLabel, noteLabel, countLabel, statusLabel;
        readonly Button copyCodeButton, readyOrStartButton, leaveButton;
        readonly MatchOptions matchOptions;
        SideTab currentSideTab = SideTab.Match;
        /// <summary>Everything the screen shows, as text: it is only rebuilt when this changes.</summary>
        string renderedSignature;
        bool canStart;

        public LobbyScreen(Ui ui) : base(ui)
        {
            // laid out in Resources/UI/Screens/LobbyScreen.uxml (UI Builder); here the buttons, the cards and the options
            Root = ui.LoadScreen("LobbyScreen", ui.root);

            // top bar: leave, room code, voice buttons, player count
            var top = Root.Q("top");
            leaveButton = Root.Q<Button>("leave"); leaveButton.clicked += () => app.LeaveRoom();
            titleLabel = Root.Q<Label>("title"); codeLabel = Root.Q<Label>("code");
            copyCodeButton = ui.Localize(Root.Q<Button>("copy"), "lobby.copy_code"); copyCodeButton.clicked += CopyCode;
            ui.settings.AddMicButton(top, "mpill");
            ui.settings.AddWhisperButton(top, "mpill");
            countLabel = Root.Q<Label>("count"); countLabel.BringToFront();   // the voice buttons come before the count

            // body: player cards, and the side panel
            playerCards = Root.Q<ScrollView>("players").contentContainer;
            playerCards.AddToClassList("lobby-cards");
            var matchButton = ui.Localize(Root.Q<Button>("tab-match"), "tab.match"); matchButton.clicked += () => ShowSideTab(SideTab.Match); matchTabButton = matchButton;
            var appearanceButton = ui.Localize(Root.Q<Button>("tab-appearance"), "tab.appearance"); appearanceButton.clicked += () => ShowSideTab(SideTab.Appearance); appearanceTabButton = appearanceButton;
            matchTab = Root.Q<ScrollView>("match-tab");
            matchOptions = new MatchOptions(ui, matchTab, Invalidate);
            appearanceTab = Root.Q<ScrollView>("appearance-tab");
            statusLabel = Root.Q<Label>("status");
            readyOrStartButton = Root.Q<Button>("ready"); readyOrStartButton.clicked += OnReadyOrStart;

            noteLabel = Root.Q<Label>("note");
            ShowSideTab(SideTab.Match);
        }

        /// <summary>Forces the next <see cref="Render"/> to rebuild (it skips when nothing it shows changed).</summary>
        public void Invalidate() => renderedSignature = null;

        /// <summary>Each time the waiting room appears. After a match ends, everyone has to say they're ready again.</summary>
        public void OnShow(bool cameFromFinal)
        {
            if (cameFromFinal) app.ready = false;
            renderedSignature = null;
        }

        void ShowSideTab(SideTab tab)
        {
            currentSideTab = tab;
            Show(matchTab, tab == SideTab.Match); Show(appearanceTab, tab == SideTab.Appearance);
            matchTabButton.EnableInClassList("tabbtn-on", tab == SideTab.Match);
            appearanceTabButton.EnableInClassList("tabbtn-on", tab == SideTab.Appearance);
            if (tab == SideTab.Appearance) ui.kidCustomizer.ShowIn(appearanceTab);
        }

        /// <summary>Guests toggle "ready"; the host starts the match once there are at least two characters.</summary>
        void OnReadyOrStart()
        {
            if (!game.isHost) { app.ToggleReady(); Invalidate(); return; }
            if (!canStart) return;
            app.StartMatch(matchOptions.BotsInMatch, game.totalRounds);
        }

        void CopyCode()
        {
            GUIUtility.systemCopyBuffer = app.roomCode;
            copyCodeButton.text = Localization.Get("lobby.copied");
            copyCodeButton.schedule.Execute(() => copyCodeButton.text = Localization.Get("lobby.copy_code")).ExecuteLater(CopiedFeedbackMs);
        }

        /// <summary>Per step while showing; rebuilds only when something it shows changed.</summary>
        public void Render()
        {
            var players = app.LobbyPlayers();
            var host = app.IsOnline ? app.FindHost() : null;
            string hostId = host != null ? host.id : app.IsOnline ? null : app.myPlayerId;
            int bots = matchOptions.BotsInMatch;
            string signature = string.Join("|", players.Select(player => player.id + player.name + Kit.Pack(player.kit) + player.ready + app.CanMutePlayer(player.id) + app.IsPlayerMuted(player.id)))
                             + hostId + game.isHost + bots + game.totalRounds + game.hideTime + game.huntTime + game.giggleInterval + game.mapId + app.ready + app.roomCode;
            if (signature == renderedSignature) return;
            renderedSignature = signature;

            bool online = app.IsOnline;
            SetText(titleLabel, Localization.Get(online ? "lobby.room" : "lobby.local"));
            SetText(codeLabel, online ? app.roomCode : ""); Show(codeLabel, online); Show(copyCodeButton, online);
            SetText(countLabel, $"{players.Count + bots}/{Rules.MaxPlayers}");
            SetText(noteLabel, Localization.Get(online ? "lobby.note_online" : "lobby.note_local"));

            playerCards.Clear();
            foreach (var player in players)
            {
                var card = AddElement(playerCards, "pcard"); if (player.isMe) card.AddToClassList("pcard-me");
                var portrait = new CharacterPortrait(62); portrait.ShowKit(player.kit); card.Add(portrait);
                AddLabel(card, player.name, "pcard-name");
                bool isHost = player.id == hostId;
                var stateLabel = AddLabel(card, Localization.Get(isHost ? "lobby.host" : player.ready ? "lobby.ready" : "lobby.waiting"), "pcard-st");
                if (isHost || player.ready) stateLabel.AddToClassList("pcard-ok");
                if (!player.isMe) MuteButton.Add(card, app, player.id, Invalidate);
            }
            if (bots > 0)
            {
                var botCard = AddElement(playerCards, "pcard", "pcard-bots");
                AddLabel(botCard, "+" + bots, "pcard-plus");
                AddLabel(botCard, Localization.Get(bots > 1 ? "lobby.bots_plural" : "lobby.bot_singular"), "pcard-st");
            }

            matchOptions.Render();

            int notReady = players.Count(player => player.id != hostId && !player.ready);
            canStart = players.Count + bots >= 2;
            string status;
            if (!game.isHost) status = Localization.Get("lobby.host_will_start");
            else if (!canStart) status = Localization.Get("lobby.need_bot");
            else status = notReady > 0 ? Localization.Get("lobby.can_start", notReady) : Localization.Get("lobby.all_ready");
            SetText(statusLabel, status);
            readyOrStartButton.text = Localization.Get(game.isHost ? "lobby.start" : app.ready ? "lobby.cancel" : "lobby.im_ready");
            readyOrStartButton.SetEnabled(!game.isHost || canStart);
            readyOrStartButton.EnableInClassList("mbtn-on", !game.isHost && app.ready);
            leaveButton.text = Localization.Get(online ? "lobby.leave" : "menu.back");
        }
    }
}
