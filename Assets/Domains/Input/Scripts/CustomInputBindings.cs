using UnityEngine;

namespace Domains.Input.Scripts
{
    public class CustomInputBindings
    {
        // Define keybindings in one place
        private const KeyCode InteractKey = KeyCode.E;
        private const KeyCode CrouchKey = KeyCode.LeftControl;
        private const KeyCode RunKey = KeyCode.LeftShift;
        private const KeyCode ChangePerspectiveKey = KeyCode.V;
        private const KeyCode PersistanceKey = KeyCode.P;
        private const KeyCode DeletionKey = KeyCode.Alpha0;
        private const KeyCode SaveDebugKey = KeyCode.F5;
        private const KeyCode PauseKey = KeyCode.Escape;
        private const int MineMouseButton = 0;
        private float eKeyHoldTime = 0f;
        private float requiredHoldDuration = 2f; // Change to desired duration in seconds


        // Methods to check input (abstraction layer)
        public static bool IsInteractPressed()
        {
            return UnityEngine.Input.GetKeyDown(InteractKey);
        }

        public static bool IsPausePressed()
        {
            return UnityEngine.Input.GetKeyDown(PauseKey);
        }

        public static bool IsSaveDebugKeyPressed()
        {
            return UnityEngine.Input.GetKeyDown(SaveDebugKey);
        }

        public static bool IsResetHeld()
        {
            return UnityEngine.Input.GetKey(InteractKey);
        }


        public static bool IsPersistanceKeyPressed()
        {
            return UnityEngine.Input.GetKeyDown(PersistanceKey);
        }

        public static bool IsDeletionKeyPressed()
        {
            return UnityEngine.Input.GetKeyDown(DeletionKey);
        }


        public static bool IsMineMouseButtonPressed()
        {
            return UnityEngine.Input.GetMouseButton(MineMouseButton);
        }

        public static bool IsChangingWeapons()
        {
            return UnityEngine.Input.mouseScrollDelta.y != 0;
        }

        public static int GetWeaponChangeDirection()
        {
            return UnityEngine.Input.mouseScrollDelta.y > 0 ? 1 : -1;
        }

        // In CustomInputBindings.cs
        public static bool IsEmergencyTeleportPressed()
        {
            // You can use any key you prefer - T for teleport is intuitive
            return UnityEngine.Input.GetKeyDown(KeyCode.R);
        }
    }
}