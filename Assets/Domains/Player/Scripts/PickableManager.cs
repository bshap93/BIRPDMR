using System;
using System.Collections.Generic;
using Domains.Scene.Scripts;
using Gameplay.Events;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace Domains.Player.Scripts
{
#if UNITY_EDITOR
    public static class PickableManagerDebug
    {
        [MenuItem("Debug/Reset Picked Items")]
        public static void ResetPickedItemsMenu()
        {
            PickableManager.ResetPickedItems();
        }
    }
#endif


    public class PickableManager : MonoBehaviour, MMEventListener<ItemEvent>
    {
        public static HashSet<string> PickedItems = new();

        private string _savePath;


        private void Start()
        {
            _savePath = GetSaveFilePath();

            if (!ES3.FileExists(_savePath))
            {
                UnityEngine.Debug.Log("[PickableManager] No save file found, forcing initial save...");
                ResetPickedItems(); // Ensure default values are set
            }

            LoadPickedItems();
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) // Press F5 to force save
                SavePickedItem("test", true);
        }


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ItemEvent eventType)
        {
            if (eventType.EventType == ItemEventType.Picked)
            {
                SavePickedItem(eventType.Item.UniqueID, true);
                UnityEngine.Debug.Log($"Item picked: {eventType.Item.BaseItem.ItemName}");
            }
        }

        private static string GetSaveFilePath()
        {
            return SaveManager.SavePickablesFileName;
        }

        public void LoadPickedItems()
        {
            if (_savePath == null)
                _savePath = GetSaveFilePath();
            var exists = ES3.FileExists(_savePath);
            if (exists)
            {
                var keys = ES3.GetKeys(_savePath);
                foreach (var key in keys)
                    if (ES3.Load<bool>(key, _savePath))
                        PickedItems.Add(key);
            }
        }

        public static void ResetPickedItems()
        {
            var saveFilePath = GetSaveFilePath();
            // Delete the Easy Save file storing picked items
            ES3.DeleteFile(GetSaveFilePath());

            // Clear the in-memory picked items list (if used)
            PickedItems.Clear();
        }


        public static bool IsItemPicked(string uniqueID)
        {
            return PickedItems.Contains(uniqueID);
        }

        public static void SavePickedItem(string uniqueID, bool b)
        {
            ES3.Save(uniqueID, b, GetSaveFilePath());
            UnityEngine.Debug.Log($"Item {uniqueID} saved as picked: {b}");
        }

        public static void SaveItemPosition(string itemPickerUniqueID, Vector3 transformPosition, string prefabName)
        {
            throw new NotImplementedException();
        }
    }
}