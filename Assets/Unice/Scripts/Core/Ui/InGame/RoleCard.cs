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

        /// <param name="card">The card's element, made in Hud.uxml.</param>
        public RoleCard(Ui ui, VisualElement card) : base(ui)
        {
            this.card = IgnorePointer(card);
            roleLabel = card.Q<Label>("role"); roleLabel.text = ""; goalLabel = card.Q<Label>("goal"); goalLabel.text = "";
            noiseBox = card.Q("noise-box");
            ui.Localize(card.Q<Label>("noise-lbl"), "hud.noise"); noiseLabel = card.Q<Label>("noise"); noiseLabel.text = "";
            noiseMeterFill = card.Q("noise-fill");
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
