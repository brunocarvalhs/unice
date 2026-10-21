using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// The waiting room's match options: the host changes them with ‹ › steppers; everyone else sees a summary line.
    /// The values live in <see cref="Game"/> and reach the guests in the snapshot.
    /// </summary>
    sealed class MatchOptions : UiPart
    {
        static readonly int[] RoundChoices = { 3, 5, 7 };
        /// <summary>Maps of stories that are not released yet (flat-tint placeholder art) are only selectable in the Editor or a dev build.</summary>
        static readonly string[] PlayableMapIds = Debug.isDebugBuild ? Maps.AllIds : new[] { MapHouse.Id };

        readonly VisualElement hostSteppers;
        readonly Label botsValue, roundsValue, hideTimeValue, huntTimeValue, giggleValue, mapValue, summaryLabel;
        readonly Action onChanged;

        public MatchOptions(Ui ui, VisualElement parent, Action onChanged) : base(ui)
        {
            this.onChanged = onChanged;
            // laid out in Resources/UI/Screens/MatchOptions.uxml (UI Builder), each row from Stepper.uxml
            var panel = ui.LoadScreen("MatchOptions", parent);
            hostSteppers = panel.Q("steppers");
            botsValue = BindStepper("bots", "lobby.bots_plural", step => game.botCount = Mathf.Clamp(Math.Min(game.botCount, MaxBots) + step, 0, MaxBots));
            roundsValue = BindStepper("rounds", "lobby.rounds_label", step =>
            {
                int index = Array.IndexOf(RoundChoices, game.totalRounds); if (index < 0) index = 1;
                game.totalRounds = RoundChoices[Mathf.Clamp(index + step, 0, RoundChoices.Length - 1)];
            });
            hideTimeValue = BindStepper("hide", "lobby.hide_label", step => game.hideTime = Mathf.Clamp(game.hideTime + step * 5, 15, 60));
            huntTimeValue = BindStepper("hunt", "lobby.hunt_label", step => game.huntTime = Mathf.Clamp(game.huntTime + step * 15, 60, 240));
            giggleValue = BindStepper("giggle", "lobby.giggle_label", step => game.giggleInterval = Mathf.Clamp(game.giggleInterval + step * 5, 5, 20));
            mapValue = BindStepper("map", "lobby.map_label", step =>
            {
                int index = Array.IndexOf(PlayableMapIds, game.mapId); if (index < 0) index = 0;
                game.mapId = PlayableMapIds[(index + step + PlayableMapIds.Length) % PlayableMapIds.Length];   // wraps around
            });
            summaryLabel = panel.Q<Label>("summary"); summaryLabel.text = "";
        }

        /// <summary>Bots fill the free seats, up to <see cref="Rules.MaxPlayers"/> in total.</summary>
        public int MaxBots => Math.Max(0, Rules.MaxPlayers - app.LobbyPlayers().Count);

        /// <summary>The bots that will actually play (the setting, capped by the free seats).</summary>
        public int BotsInMatch => Math.Min(game.botCount, MaxBots);

        /// <summary>Wires the "label  ‹ value ›" row named <paramref name="rowName"/> (a Stepper.uxml instance); returns its value label.</summary>
        Label BindStepper(string rowName, string labelKey, Action<int> change)
        {
            var stepper = hostSteppers.Q(rowName);
            ui.Localize(stepper.Q<Label>("label"), labelKey);
            stepper.Q<Button>("less").clicked += () => { change(-1); onChanged(); };
            stepper.Q<Button>("more").clicked += () => { change(1); onChanged(); };
            var value = stepper.Q<Label>("value"); value.text = "";
            return value;
        }

        public void Render()
        {
            int bots = BotsInMatch; string mapName = Localization.Get("map." + game.mapId + ".name");
            SetText(botsValue, bots.ToString()); SetText(roundsValue, game.totalRounds.ToString());
            SetText(hideTimeValue, $"{game.hideTime:0}s"); SetText(huntTimeValue, $"{game.huntTime:0}s"); SetText(giggleValue, $"{game.giggleInterval:0}s");
            SetText(mapValue, mapName);
            Show(hostSteppers, game.isHost);
            SetText(summaryLabel, game.isHost ? "" : Localization.Get("lobby.info", bots, game.totalRounds, game.hideTime, game.huntTime, game.giggleInterval, mapName));
            Show(summaryLabel, !game.isHost);
        }
    }
}
