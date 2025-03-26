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
        private const int MineMouseButton = 0;

        // Methods to check input (abstraction layer)
        public static bool IsInteractPressed()
        {
            return UnityEngine.Input.GetKeyDown(InteractKey);
        }


        public static bool IsCrouchPressed()
        {
            return UnityEngine.Input.GetKey(CrouchKey);
        }

        public static bool IsRunHeld()
        {
            return UnityEngine.Input.GetKey(RunKey);
        }

        public static bool IsChangePerspectivePressed()
        {
            return UnityEngine.Input.GetKeyDown(ChangePerspectiveKey);
        }

        public static bool IsPersistanceKeyPressed()
        {
            return UnityEngine.Input.GetKeyDown(PersistanceKey);
        }

        public static bool IsDeletionKeyPressed()
        {
            return UnityEngine.Input.GetKeyDown(DeletionKey);
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
            return UnityEngine.Input.GetMouseButton(MineMouseButton);
        }
    }
}