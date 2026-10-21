using System;
using UnityEngine;
using UnityEngine.UIElements;
using static Unice.UiKit;

namespace Unice
{
    /// <summary>The on-screen controls on phones: the joystick on the left and the round action buttons on the right.</summary>
    sealed class TouchControls : UiPart
    {
        /// <summary>Opacity of a button that shows but can't be used right now.</summary>
        const float UnavailableOpacity = .45f;

        public readonly VisualElement Root;
        readonly Joystick joystick;
        readonly VisualElement useButton, passButton, sneakButton, laughButton, useButtonIcon;
        ActionIcon useButtonIconKind = ActionIcon.Door;

        public Vector2 JoystickDirection => joystick.Direction;
        /// <summary>The sneak button is a toggle (keyboard players hold Shift instead).</summary>
        public bool SneakToggled { get; private set; }

        public TouchControls(Ui ui) : base(ui)
        {
            // the round buttons are laid out in Resources/UI/Screens/TouchControls.uxml (UI Builder)
            Root = IgnorePointer(ui.LoadScreen("TouchControls", ui.safeArea));
            joystick = new Joystick(Root);

            var buttons = Root.Q("acts"); buttons.BringToFront();   // the joystick was added after it: the buttons stay on top
            sneakButton = BindActionButton(buttons.Q("sneak"), ActionIcon.Sneak, () => { SneakToggled = !SneakToggled; sneakButton.EnableInClassList("act-on", SneakToggled); });
            laughButton = BindActionButton(buttons.Q("laugh"), ActionIcon.Laugh, () => app.TryLaugh());
            passButton = BindActionButton(buttons.Q("pass"), ActionIcon.Pass, () => app.TryPassDoll());
            useButton = BindActionButton(buttons.Q("use"), ActionIcon.Door, () => app.UseNearbySpot());
            // the use button's icon changes (door for hiders, eye for the Unice), so it is drawn from a field
            useButtonIcon = useButton.Q(className: "act-icon");
            useButtonIcon.generateVisualContent = context => ActionIcons.Draw(context, useButtonIconKind, useButton.resolvedStyle.color);
        }

        /// <summary>No finger on the joystick and sneak off, so nothing keeps moving the character on its own.</summary>
        public void Release() { joystick.Cancel(); SneakToggled = false; sneakButton.EnableInClassList("act-on", false); }

        /// <summary>Shows the buttons that apply right now; the ones that can't be used yet are dimmed.</summary>
        public void ShowAvailable(ActionHints hints)
        {
            Show(useButton, hints.active); Show(passButton, hints.showPass); Show(laughButton, hints.showLaugh); Show(sneakButton, hints.active && !hints.isUnice);
            if (!hints.active) return;
            SetUseButtonIcon(hints.isUnice ? ActionIcon.Eye : ActionIcon.Door);
            useButton.style.opacity = hints.canUseSpot ? 1 : UnavailableOpacity;
            passButton.style.opacity = hints.canPass ? 1 : UnavailableOpacity;
            laughButton.style.opacity = hints.canLaugh ? 1 : UnavailableOpacity;
        }

        void SetUseButtonIcon(ActionIcon icon)
        {
            if (useButtonIconKind == icon) return;
            useButtonIconKind = icon; useButtonIcon.MarkDirtyRepaint();
        }

        /// <summary>A round touch button with a vector icon; it fires on touch down (not on release) so it feels immediate.</summary>
        VisualElement BindActionButton(VisualElement button, ActionIcon icon, Action onPress)
        {
            var iconElement = IgnorePointer(button.Q(className: "act-icon"));
            iconElement.generateVisualContent += context => ActionIcons.Draw(context, icon, button.resolvedStyle.color);
            button.RegisterCallback<PointerDownEvent>(e => { onPress(); e.StopPropagation(); });
            return button;
        }
    }
}
