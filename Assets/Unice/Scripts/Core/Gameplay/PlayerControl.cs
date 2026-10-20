using UnityEngine;
using UnityEngine.InputSystem;

namespace Unice
{
    /// <summary>
    /// This device's player, input side: reads the keyboard, the gamepad and the joystick and hands them to the
    /// <see cref="PlayerController"/> (which moves the character and runs the E/Q/R actions); also Esc and P.
    /// </summary>
    sealed class PlayerControl
    {
        readonly App app;
        readonly PlayerController controller;
        Game game => app.game;
        bool upHeld, downHeld, leftHeld, rightHeld, sneakHeld;
        Vector2 padStick;

        public int actionSeq => controller.actionSeq;
        public int actionArg => controller.actionArg;
        public PlayerAction action => controller.action;

        public PlayerControl(App app, PlayerController controller) { this.app = app; this.controller = controller; }

        public void Reset() => controller.Reset();
        public void TickCooldowns(float dt) => controller.TickCooldowns(dt);
        public void UpdateNoiseLevel(float dt) => controller.UpdateNoiseLevel(dt);
        public void UseNearbySpot() => controller.UseNearbySpot();
        public void TryPassDoll() => controller.TryPassDoll();
        public void TryLaugh() => controller.TryLaugh();

        /// <summary>Per frame: which keys are held, plus the one-shot keys (E/Q/R/P, Esc).</summary>
        public void ReadKeys()
        {
            var keyboard = Keyboard.current; var pad = Gamepad.current;
            padStick = Vector2.zero; sneakHeld = false;
            if (keyboard == null) { upHeld = downHeld = leftHeld = rightHeld = sneakHeld = false; if (pad == null) return; }
            if (pad != null) ReadGamepad(pad);
            if (keyboard == null) return;
            bool typing = app.ui.Typing;   // keys go to the text field
            upHeld = !typing && (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed);
            downHeld = !typing && (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
            leftHeld = !typing && (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
            rightHeld = !typing && (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed);
            sneakHeld = sneakHeld || keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            if (keyboard.escapeKey.wasPressedThisFrame) app.Back();   // also the Android back button
            if (typing || !game.IsLive || game.InIntro || app.paused) return;
            if (keyboard.eKey.wasPressedThisFrame) controller.UseNearbySpot();
            if (keyboard.qKey.wasPressedThisFrame) controller.TryPassDoll();
            if (keyboard.rKey.wasPressedThisFrame) controller.TryLaugh();
            if (keyboard.pKey.wasPressedThisFrame) app.TogglePause();
        }

        /// <summary>
        /// Gamepad: left stick or d-pad walks, a trigger or shoulder sneaks, A (south) uses the nearby spot, X (west) passes the doll,
        /// Y (north) laughs, Start pauses and B (east) goes back. The menus are driven by the UI's own navigation.
        /// </summary>
        void ReadGamepad(Gamepad pad)
        {
            padStick = pad.leftStick.ReadValue();
            var dpad = pad.dpad.ReadValue();
            if (dpad != Vector2.zero) padStick = dpad;
            padStick.y = -padStick.y;   // the stick is up-positive, the game's y grows downwards
            sneakHeld = pad.leftTrigger.isPressed || pad.rightTrigger.isPressed || pad.leftShoulder.isPressed || pad.rightShoulder.isPressed;
            if (pad.buttonEast.wasPressedThisFrame) app.Back();
            if (app.ui.Typing || !game.IsLive || game.InIntro || app.paused) return;
            if (pad.buttonSouth.wasPressedThisFrame) controller.UseNearbySpot();
            if (pad.buttonWest.wasPressedThisFrame) controller.TryPassDoll();
            if (pad.buttonNorth.wasPressedThisFrame) controller.TryLaugh();
            if (pad.startButton.wasPressedThisFrame) app.TogglePause();
        }

        /// <summary>Keys, gamepad and joystick, as a unit direction (zero inside the dead zone).</summary>
        public Vector2 MoveInput()
        {
            var joystick = app.ui.JoystickDirection;
            var direction = PlayerController.Direction((rightHeld ? 1 : 0) - (leftHeld ? 1 : 0) + padStick.x, (downHeld ? 1 : 0) - (upHeld ? 1 : 0) + padStick.y, joystick.x, joystick.y);
            return new Vector2(direction.x, direction.y);
        }

        /// <summary>Moves this player from the input.</summary>
        public void Move(float dt)
        {
            if (game.InIntro) return;   // the opening video is playing
            var direction = MoveInput();
            controller.Move(dt, (direction.x, direction.y), sneakHeld || app.ui.SneakToggled);
        }
    }
}
