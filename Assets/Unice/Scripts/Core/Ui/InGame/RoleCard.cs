using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>
    /// HUD card under the map/menu buttons: your role and goal, and how much
    /// noise you are making (only while you are making some).
    /// </summary>
    sealed class RoleCard : UiPart
    {
        /// <summary>The goal line is shown this long (real seconds) each time your role or the phase changes, then it folds away.</summary>
        const float GoalShownSeconds = 10;

        static readonly string[] RoleClasses = { "role-unice", "role-out", "role-holder" };

        readonly VisualElement card, noiseBox, noiseMeterFill;
        string goalKey; float goalSince;
        readonly Label roleLabel, goalLabel, noiseLabel;

        public RoleCard(Ui ui, VisualElement parent) : base(ui)
        {
            card = IgnorePointer(AddElement(parent, "hud-card", "hud-tr"));
            roleLabel = AddLabel(card, "", "role-t"); goalLabel = AddLabel(card, "", "role-g");
            noiseBox = AddElement(card); noiseBox.style.marginTop = 8;
            var noiseRow = AddElement(noiseBox, "row", "wrap");
            LocalizedLabel(noiseRow, "hud.noise", "hud-lbl"); noiseLabel = AddLabel(noiseRow, "", "hud-lbl");
            var noiseMeter = AddElement(noiseBox, "meter", "meter-noise"); noiseMeterFill = AddElement(noiseMeter, "meter-fill");
            foreach (var label in card.Query<Label>().ToList()) IgnorePointer(label);
        }

        /// <summary>Refreshes from <paramref name="data"/> (the Hud computes it once for this card and the match card).</summary>
        public void Refresh(RoleCardData data)
        {
            SetText(roleLabel, data.role); SetText(goalLabel, data.goal);
            string key = data.role + data.goal;
            if (key != goalKey) { goalKey = key; goalSince = Time.unscaledTime; }
            Show(goalLabel, Time.unscaledTime - goalSince < GoalShownSeconds);
            string roleClass = data.style switch { RoleStyle.Unice => "role-unice", RoleStyle.Out => "role-out", RoleStyle.Holder => "role-holder", _ => "" };
            foreach (var cssClass in RoleClasses) card.EnableInClassList(cssClass, cssClass == roleClass);

            Show(noiseBox, data.showNoise);
            noiseMeterFill.style.width = Length.Percent(data.noiseProgress * 100);
            SetText(noiseLabel, data.noiseText ?? "");
        }
    }
}
