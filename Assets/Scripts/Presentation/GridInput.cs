using UnityEngine;
using UnityEngine.InputSystem;

namespace CrystalRuins.Presentation
{
    // Keyboard and gamepad (Xbox) reading, with key repeat for held directions.
    public sealed class GridInput
    {
        const float RepeatDelay = 0.12f;
        const float StickDeadZone = 0.5f;

        Vector2Int lastDir;
        float nextRepeat;

        public Vector2Int PollMove()
        {
            var dir = ReadDirection();
            if (dir == Vector2Int.zero)
            {
                lastDir = dir;
                return dir;
            }

            if (dir == lastDir && Time.unscaledTime < nextRepeat) return Vector2Int.zero;

            lastDir = dir;
            nextRepeat = Time.unscaledTime + RepeatDelay;
            return dir;
        }

        public bool Undo()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            return (k != null && (k.zKey.wasPressedThisFrame || k.backspaceKey.wasPressedThisFrame))
                || (g != null && g.buttonEast.wasPressedThisFrame);
        }

        public bool Restart()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            return (k != null && k.rKey.wasPressedThisFrame)
                || (g != null && g.buttonNorth.wasPressedThisFrame);
        }

        public bool Pause()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            return (k != null && (k.escapeKey.wasPressedThisFrame || k.pKey.wasPressedThisFrame))
                || (g != null && g.startButton.wasPressedThisFrame);
        }

        static Vector2Int ReadDirection()
        {
            Vector2 v = Vector2.zero;

            var k = Keyboard.current;
            if (k != null)
            {
                if (k.aKey.isPressed || k.leftArrowKey.isPressed) v.x -= 1;
                if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1;
                if (k.sKey.isPressed || k.downArrowKey.isPressed) v.y -= 1;
                if (k.wKey.isPressed || k.upArrowKey.isPressed) v.y += 1;
            }

            var g = Gamepad.current;
            if (g != null)
            {
                v += g.dpad.ReadValue();
                var stick = g.leftStick.ReadValue();
                if (stick.magnitude > StickDeadZone) v += stick;
            }

            if (v == Vector2.zero) return Vector2Int.zero;

            // Grid movement is 4-directional: keep the dominant axis.
            if (Mathf.Abs(v.x) >= Mathf.Abs(v.y))
                return new Vector2Int(v.x > 0 ? 1 : -1, 0);
            return new Vector2Int(0, v.y > 0 ? 1 : -1);
        }
    }
}
