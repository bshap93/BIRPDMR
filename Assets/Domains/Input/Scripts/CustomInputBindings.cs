using UnityEngine;

namespace Domains.Input.Scripts
{
    public class CustomInputBindings
    {
        // Define keybindings in one place
        private static readonly KeyCode interactKey = KeyCode.E;
        private static readonly KeyCode crouchKey = KeyCode.LeftControl;
        private static readonly KeyCode runKey = KeyCode.LeftShift;
        private static KeyCode alpha1Key = KeyCode.Alpha1;
        private static readonly KeyCode changePerspectiveKey = KeyCode.V;
        private static readonly KeyCode persistanceKey = KeyCode.P;
        private static readonly KeyCode deletionKey = KeyCode.Alpha0;
        private static readonly int mineMouseButton = 0;

        // Methods to check input (abstraction layer)
        public static bool IsInteractPressed()
        {
            return UnityEngine.Input.GetKeyDown(interactKey);
        }


        public static bool IsCrouchPressed()
        {
            return UnityEngine.Input.GetKey(crouchKey);
        }

        public static bool IsRunHeld()
        {
            return UnityEngine.Input.GetKey(runKey);
        }

        public static bool IsChangePerspectivePressed()
        {
            return UnityEngine.Input.GetKeyDown(changePerspectiveKey);
        }

        public static bool IsPersistanceKeyPressed()
        {
            return UnityEngine.Input.GetKeyDown(persistanceKey);
        }

        public static bool IsDeletionKeyPressed()
        {
            return UnityEngine.Input.GetKeyDown(deletionKey);
        }

        public static int GetPressedNumberKey()
        {
            for (var i = 0; i < 9; i++) // Checks keys 1-9
                if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1 + i) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.Keypad1 + i))
                    return i; // Returns the number key that was pressed (0 = "1", 1 = "2", etc.)

            return -1; // No number key was pressed
        }


        public static bool IsMineMouseButtonPressed()
        {
            return UnityEngine.Input.GetMouseButton(mineMouseButton);
        }
    }
}