using UnityEngine.InputSystem;

namespace Unice
{
    /// <summary>
    /// Whether the player is using touch or keyboard and mouse, decided by the last device they actually used instead of the
    /// platform: a phone with a keyboard, a laptop with a touch screen and a browser on a phone all behave. It starts from
    /// the platform until the first input. Touch shows the on-screen controls; keyboard, mouse and gamepad show key hints
    /// (the gamepad also drives the menus by focus, so <see cref="Gamepad"/> tells the UI to keep one button focused).
    /// </summary>
    sealed class InputMode
    {
        /// <summary>Mouse movement below this many pixels in a frame is noise, not a hand on the mouse.</summary>
        const float MouseMoveThreshold = 2;

        /// <summary>True while touch is the player's input.</summary>
        public bool Touch { get; private set; }
        /// <summary>True while a gamepad is the player's input (never together with <see cref="Touch"/>).</summary>
        public bool Gamepad { get; private set; }

        /// <summary>Stick deflection from which a gamepad counts as in hand (drift stays below it).</summary>
        const float StickThreshold = .5f;

        public InputMode(bool startWithTouch) { Touch = startWithTouch; }

        /// <summary>Sets the mode by hand (layout screenshots); the next real input can change it again.</summary>
        public void Force(bool touch) { Touch = touch; Gamepad = false; }

        static bool PadUsed(UnityEngine.InputSystem.Gamepad pad) =>
            pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame
                || pad.startButton.wasPressedThisFrame || pad.dpad.up.wasPressedThisFrame || pad.dpad.down.wasPressedThisFrame || pad.dpad.left.wasPressedThisFrame || pad.dpad.right.wasPressedThisFrame
                || pad.leftStick.ReadValue().sqrMagnitude > StickThreshold * StickThreshold);

        /// <summary>Per frame: looks at what was used this frame. Returns true when the mode changed.</summary>
        public bool Update()
        {
            var touchscreen = Touchscreen.current; var keyboard = Keyboard.current; var mouse = Mouse.current;
            bool touched = touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame;
            bool fingerDown = touchscreen != null && touchscreen.primaryTouch.press.isPressed;
            bool keys = keyboard != null && keyboard.anyKey.wasPressedThisFrame;
            // a touch can be mirrored as mouse events: the mouse only counts while no finger is down
            bool pointer = !touched && !fingerDown && mouse != null
                && (mouse.leftButton.wasPressedThisFrame || mouse.delta.ReadValue().sqrMagnitude > MouseMoveThreshold * MouseMoveThreshold);

            bool padUsed = PadUsed(UnityEngine.InputSystem.Gamepad.current);
            bool touch = Touch, gamepad = Gamepad;
            if (touched) { touch = true; gamepad = false; }
            else if (keys || pointer) { touch = false; gamepad = false; }
            else if (padUsed) { touch = false; gamepad = true; }
            if (touch == Touch && gamepad == Gamepad) return false;
            Touch = touch; Gamepad = gamepad;
            return true;
        }
    }
}
