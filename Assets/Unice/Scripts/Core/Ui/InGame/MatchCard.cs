using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>HUD top-left card: the clock with the round and phase beside it.</summary>
    sealed class MatchCard : UiPart
    {
        /// <summary>The clock turns red in the last seconds of the hunt.</summary>
        const float LowTimeSeconds = 15;

        readonly Label roundLabel, clockLabel, phaseLabel;

        public MatchCard(Ui ui, VisualElement parent) : base(ui)
        {
            var card = IgnorePointer(AddElement(parent, "hud-card", "hud-tl"));
            var top = AddElement(card, "row");
            clockLabel = AddLabel(top, "0:30", "clock");
            var side = AddElement(top, "hud-side");
            var roundRow = AddElement(side, "row");
            LocalizedLabel(roundRow, "hud.round", "hud-lbl"); roundLabel = AddLabel(roundRow, "", "hud-lbl");
            phaseLabel = AddLabel(side, "", "phase");
            foreach (var label in card.Query<Label>().ToList()) IgnorePointer(label);
        }

        public void Refresh()
        {
            var me = app.myCharacter; bool live = game.IsLive;
            SetText(roundLabel, $"{game.round}/{game.totalRounds}");
            SetText(clockLabel, TextFormat.FormatMinutesSeconds(live ? game.timeLeft : 0));
            clockLabel.EnableInClassList("clock-low", game.phase == Phase.Hunt && game.timeLeft <= LowTimeSeconds);

            string phaseKey = game.phase switch
            {
                Phase.Hide => "hud.phase_hide",
                Phase.Hunt => game.unice == me ? "hud.phase_hunt_you" : "hud.phase_hunt_other",
                Phase.Result => "hud.phase_result",
                _ => null,
            };
            SetText(phaseLabel, phaseKey != null ? Localization.Get(phaseKey) : "");
        }
    }
}
