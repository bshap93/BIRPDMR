using System;
using Domains.Items;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Domains.Debug
{
    public class ManualItemPickerIDAssigner : Editor
    {
        [MenuItem("Debug/Assign Unique IDs to ItemPickers")]
        private static void AssignUniqueIDs()
        {
            // Find all ManualItemPicker components in the current scene
            var allItemPickers = FindObjectsByType<ItemPicker>(FindObjectsSortMode.None);

            // Check if any were found
            if (allItemPickers.Length == 0)
            {
                UnityEngine.Debug.LogWarning("No ManualItemPicker components found in the scene.");
                return;
            }

            // Iterate through each ManualItemPicker and assign a unique ID
            foreach (var picker in allItemPickers)
                if (picker != null)
                {
                    // Generate a unique ID using GUID
                    picker.uniqueID = Guid.NewGuid().ToString();
                    EditorUtility.SetDirty(picker); // Mark the object as dirty for saving
                }

            // Save the scene to persist changes
            AssetDatabase.SaveAssets();

            UnityEngine.Debug.Log($"Assigned unique IDs to {allItemPickers.Length} ManualItemPicker components.");
        }
    }
}
#endif