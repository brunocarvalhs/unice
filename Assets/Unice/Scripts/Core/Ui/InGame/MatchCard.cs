using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>HUD top-left card: the clock with the round and phase beside it.</summary>
    sealed class MatchCard : UiPart
    {
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
            var data = HudModels.MatchCard(game, app.myCharacter);
            SetText(roundLabel, data.round);
            SetText(clockLabel, data.clock);
            clockLabel.EnableInClassList("clock-low", data.clockLow);
            SetText(phaseLabel, data.phase);
        }
    }
}
