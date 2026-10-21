using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>HUD top-left card: the clock with the round and phase beside it, and under it how close the doll is to giggling.</summary>
    sealed class MatchCard : UiPart
    {
        readonly Label roundLabel, clockLabel, phaseLabel, giggleNoteLabel;
        readonly VisualElement giggleMeter, giggleMeterFill;

        /// <param name="card">The card's element, made in Hud.uxml.</param>
        public MatchCard(Ui ui, VisualElement card) : base(ui)
        {
            IgnorePointer(card);
            clockLabel = card.Q<Label>("clock");
            ui.Localize(card.Q<Label>("round-lbl"), "hud.round"); roundLabel = card.Q<Label>("round"); roundLabel.text = "";
            phaseLabel = card.Q<Label>("phase"); phaseLabel.text = "";
            giggleMeter = card.Q("meter"); giggleMeterFill = card.Q("meter-fill");
            giggleNoteLabel = card.Q<Label>("note");
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
