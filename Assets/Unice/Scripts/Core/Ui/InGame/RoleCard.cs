using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// HUD card under the map/menu buttons: your role and goal, who has the doll (if your team can know), when it giggles next,
    /// and how much noise you are making (only while you are making some).
    /// </summary>
    sealed class RoleCard : UiPart
    {
        /// <summary>The goal line is shown this long (real seconds) each time your role or the phase changes, then it folds away.</summary>
        const float GoalShownSeconds = 10;

        static readonly string[] RoleClasses = { "role-unice", "role-out", "role-holder" };

        readonly VisualElement card, giggleMeter, giggleMeterFill, noiseBox, noiseMeterFill;
        string goalKey; float goalSince;
        readonly Label roleLabel, goalLabel, dollHolderLabel, dollNoteLabel, noiseLabel;

        public RoleCard(Ui ui, VisualElement parent) : base(ui)
        {
            card = IgnorePointer(AddElement(parent, "hud-card", "hud-tr"));
            roleLabel = AddLabel(card, "", "role-t"); goalLabel = AddLabel(card, "", "role-g");
            var dollRow = AddElement(card, "row", "wrap"); dollRow.style.marginTop = 8;
            LocalizedLabel(dollRow, "hud.doll", "hud-lbl"); dollHolderLabel = AddLabel(dollRow, "", "hud-lbl");
            giggleMeter = AddElement(card, "meter"); giggleMeterFill = AddElement(giggleMeter, "meter-fill");
            dollNoteLabel = AddLabel(card, "", "note");
            noiseBox = AddElement(card); noiseBox.style.marginTop = 8;
            var noiseRow = AddElement(noiseBox, "row", "wrap");
            LocalizedLabel(noiseRow, "hud.noise", "hud-lbl"); noiseLabel = AddLabel(noiseRow, "", "hud-lbl");
            var noiseMeter = AddElement(noiseBox, "meter", "meter-noise"); noiseMeterFill = AddElement(noiseMeter, "meter-fill");
            foreach (var label in card.Query<Label>().ToList()) IgnorePointer(label);
        }

        public void Refresh()
        {
            var data = HudModels.RoleCard(game, app.myCharacter, app.perspective);

            SetText(roleLabel, data.role); SetText(goalLabel, data.goal);
            string key = data.role + data.goal;
            if (key != goalKey) { goalKey = key; goalSince = Time.unscaledTime; }
            Show(goalLabel, Time.unscaledTime - goalSince < GoalShownSeconds);
            string roleClass = data.style switch { RoleStyle.Unice => "role-unice", RoleStyle.Out => "role-out", RoleStyle.Holder => "role-holder", _ => "" };
            foreach (var cssClass in RoleClasses) card.EnableInClassList(cssClass, cssClass == roleClass);

            SetText(dollHolderLabel, data.dollHolder);
            Show(giggleMeter, data.showGiggleMeter);
            giggleMeterFill.style.width = Length.Percent(data.giggleProgress * 100);
            SetText(dollNoteLabel, data.note);
            Show(dollNoteLabel, data.note.Length > 0);

            Show(noiseBox, data.showNoise);
            noiseMeterFill.style.width = Length.Percent(data.noiseProgress * 100);
            SetText(noiseLabel, data.noiseText ?? "");
        }
    }
}
