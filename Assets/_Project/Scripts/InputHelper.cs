using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ColorGateBlast
{
    /// <summary>Single pointer abstraction (mouse or first touch) that works with either input backend.</summary>
    public static class InputHelper
    {
        public static bool PressedThisFrame
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Pointer p = Pointer.current;
                return p != null && p.press.wasPressedThisFrame;
#else
                return Input.GetMouseButtonDown(0);
#endif
            }
        }

        public static bool IsPressed
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Pointer p = Pointer.current;
                return p != null && p.press.isPressed;
#else
                return Input.GetMouseButton(0);
#endif
            }
        }

        public static bool ReleasedThisFrame
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Pointer p = Pointer.current;
                return p != null && p.press.wasReleasedThisFrame;
#else
                return Input.GetMouseButtonUp(0);
#endif
            }
        }

        public static Vector2 PointerPosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                Pointer p = Pointer.current;
                return p != null ? p.position.ReadValue() : Vector2.zero;
#else
                return Input.mousePosition;
#endif
            }
        }

        /// <summary>True when the pointer is on a UI element (buttons must not start block drags).</summary>
        public static bool OverUi
        {
            get { return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(); }
        }
    }
}
