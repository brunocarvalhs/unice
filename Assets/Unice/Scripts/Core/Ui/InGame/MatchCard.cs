using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>HUD top-left card: the clock with the round and phase beside it, and under it how close the doll is to giggling.</summary>
    sealed class MatchCard : UiPart
    {
        readonly Label roundLabel, clockLabel, phaseLabel, giggleNoteLabel;
        readonly VisualElement giggleMeter, giggleMeterFill;

        public MatchCard(Ui ui, VisualElement parent) : base(ui)
        {
            var card = IgnorePointer(AddElement(parent, "hud-card", "hud-tl"));
            var top = AddElement(card, "row");
            clockLabel = AddLabel(top, "0:30", "clock");
            var side = AddElement(top, "hud-side");
            var roundRow = AddElement(side, "row");
            LocalizedLabel(roundRow, "hud.round", "hud-lbl"); roundLabel = AddLabel(roundRow, "", "hud-lbl");
            phaseLabel = AddLabel(side, "", "phase");
            giggleMeter = AddElement(card, "meter"); giggleMeterFill = AddElement(giggleMeter, "meter-fill");
            giggleNoteLabel = AddLabel(card, "", "note");
            foreach (var label in card.Query<Label>().ToList()) IgnorePointer(label);
        }

        /// <param name="giggle">The giggle meter and note come from the same data as the role card.</param>
        public void Refresh(RoleCardData giggle)
        {
            var data = HudModels.MatchCard(game, app.myCharacter);
            SetText(roundLabel, data.round);
            SetText(clockLabel, data.clock);
            clockLabel.EnableInClassList("clock-low", data.clockLow);
            SetText(phaseLabel, data.phase);
            Show(giggleMeter, giggle.showGiggleMeter);
            giggleMeterFill.style.width = Length.Percent(giggle.giggleProgress * 100);
            SetText(giggleNoteLabel, giggle.note);
            Show(giggleNoteLabel, giggle.note.Length > 0);
        }
    }
}
