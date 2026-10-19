using UnityEngine.InputSystem;

namespace Unice
{
    /// <summary>
    /// Whether the player is using touch or keyboard and mouse, decided by the last device they actually used instead of the
    /// platform: a phone with a keyboard, a laptop with a touch screen and a browser on a phone all behave. It starts from
    /// the platform until the first input. Touch shows the on-screen controls; keyboard and mouse show key hints.
    /// </summary>
    sealed class InputMode
    {
        /// <summary>Mouse movement below this many pixels in a frame is noise, not a hand on the mouse.</summary>
        const float MouseMoveThreshold = 2;

        /// <summary>True while touch is the player's input.</summary>
        public bool Touch { get; private set; }

        public InputMode(bool startWithTouch) { Touch = startWithTouch; }

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

            bool touch = Touch;
            if (touched) touch = true;
            else if (keys || pointer) touch = false;
            if (touch == Touch) return false;
            Touch = touch;
            return true;
        }
    }
}
