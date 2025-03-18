using System;
using System.Collections.Generic;
using System.Linq;
using Domains.Items;
using Domains.Items.Events;
using Domains.UI;
using JetBrains.Annotations;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace Domains.Scene.Scripts
{
#if UNITY_EDITOR
    public static class InventoryPersistenceManagerDebug
    {
        [MenuItem("Debug/Reset/Reset Inventory")]
        public static void ResetInventory()
        {
            PlayerInventoryManager.ResetInventory();
        }
    }
#endif

    public class PlayerInventoryManager : MonoBehaviour, MMEventListener<InventoryEvent>
    {
        private const string InventoryKey = "InventoryContentData";
        private const string ResourcesPath = "Items";
        public static Inventory PlayerInventory;

        public static List<InventoryEntryData> InventoryContentData = new();

        // Add this to PlayerInventoryManager
        [SerializeField] private List<InventoryEntryData> _currentInventoryItems = new();

        [CanBeNull] public InventoryBarUpdater inventoryBarUpdater;

        private string _savePath;

        private void Start()
        {
            PlayerInventory = FindFirstObjectByType<Inventory>();

            if (PlayerInventory == null)
            {
                UnityEngine.Debug.LogError("Failed to find Inventory component in scene!");
                return; // Early return to prevent null reference exceptions
            }

            _savePath = GetSaveFilePath();

            if (!ES3.FileExists(_savePath))
            {
                UnityEngine.Debug.Log("[PlayerInventoryManager] No save file found, forcing initial save...");
                ResetInventory(); // Ensure default values are set
            }

            PlayerInventory.SetWeightLimit(PlayerInfoSheet.WeightLimit);

            LoadInventory();
        }

// Then in Update() method (or you could do this in LateUpdate to be more efficient)
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) // Press F5 to force save
            {
                SaveInventory();
                UnityEngine.Debug.Log("Player inventory saved");
            }
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(InventoryEvent eventType)
        {
            UnityEngine.Debug.Log($"Inventory event received: {eventType.EventType}");

            // Add this to save when inventory changes
            if (eventType.EventType == InventoryEventType.ContentChanged) SaveInventory();

            if (eventType.EventType == InventoryEventType.SellAllItems)
            {
                PlayerInventory.SellAllItems();
                SaveInventory();
            }
        }


        private static string GetSaveFilePath()
        {
            return SaveManager.SaveFileName;
        }

        public static void SaveInventory()
        {
            // Skip saving if in Editor mode and not playing
            if (!Application.isPlaying)
            {
                UnityEngine.Debug.Log("SaveInventory skipped in Editor mode");
                return;
            }

            var saveFilePath = GetSaveFilePath();

            if (PlayerInventory == null)
            {
                UnityEngine.Debug.LogError("InventoryPersistenceManager: No Inventory Assigned!");
                return;
            }

            var inventoryData = new List<InventoryEntryData>();

            foreach (var entry in PlayerInventory.Content)
                inventoryData.Add(new InventoryEntryData(entry.UniqueID, entry.BaseItem.ItemID));

            InventoryContentData = inventoryData; // Update static reference too
            ES3.Save(InventoryKey, inventoryData, saveFilePath);
            UnityEngine.Debug.Log($"✅ Inventory saved at {saveFilePath}");
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(GetSaveFilePath());
        }

        public void Initialize()
        {
            ResetInventory();
            if (inventoryBarUpdater != null) inventoryBarUpdater.Initialize();
        }

        public void LoadInventory()
        {
            var saveFilePath = GetSaveFilePath();

            if (ES3.FileExists(saveFilePath) && ES3.KeyExists(InventoryKey, saveFilePath))
            {
                InventoryContentData = ES3.Load<List<InventoryEntryData>>(InventoryKey, saveFilePath);
                if (inventoryBarUpdater != null) inventoryBarUpdater.Initialize();
                PlayerInventory.Content.Clear();

                foreach (var itemData in InventoryContentData)
                {
                    var item = GetItemByID(itemData.ItemID);
                    if (item != null)
                    {
                        var entry = new Inventory.InventoryEntry(itemData.UniqueID, item);
                        PlayerInventory.Content.Add(entry);
                    }
                }

                UnityEngine.Debug.Log($"✅ Loaded inventory data from {saveFilePath}");
            }
            else
            {
                UnityEngine.Debug.Log(
                    $"No saved inventory data found at {saveFilePath} or key doesn't exist. Creating new inventory.");
                InventoryContentData = new List<InventoryEntryData>();
                PlayerInventory.Content.Clear();
                SaveInventory(); // Create an empty inventory save
            }
        }

        public static void ResetInventory()
        {
            InventoryContentData = new List<InventoryEntryData>();

            // Only try to clear the inventory if we're in Play mode with an active inventory
            if (Application.isPlaying && PlayerInventory != null) PlayerInventory.Content.Clear();

            // Special case for when called from Editor
            if (!Application.isPlaying)
            {
                // Just delete the saved inventory data without trying to access runtime objects
                var saveFilePath = GetSaveFilePath();
                if (ES3.FileExists(saveFilePath) && ES3.KeyExists(InventoryKey, saveFilePath))
                {
                    ES3.DeleteKey(InventoryKey, saveFilePath);
                    UnityEngine.Debug.Log($"Deleted inventory data from {saveFilePath}");
                }

                return; // Skip the SaveInventory call below
            }

            SaveInventory();
        }

        private static void RestoreInventory(Inventory inventory, List<InventoryEntryData> inventoryContentData)
        {
            foreach (var itemData in inventoryContentData)
            {
                var item = GetItemByID(itemData.ItemID);
                if (item != null)
                {
                    var entry = new Inventory.InventoryEntry(itemData.UniqueID, item); // Use saved UniqueID
                    inventory.AddItem(entry);
                }
            }
        }

        private static BaseItem GetItemByID(string itemID)
        {
            return Resources.LoadAll<BaseItem>(ResourcesPath).FirstOrDefault(i => i.ItemID == itemID);
        }

        [Serializable]
        public class InventoryEntryData
        {
            public string UniqueID;
            public string ItemID;

            public InventoryEntryData(string itemUniqueID, string baseItemItemID)
            {
                UniqueID = itemUniqueID;
                ItemID = baseItemItemID;
            }
        }
    }
}