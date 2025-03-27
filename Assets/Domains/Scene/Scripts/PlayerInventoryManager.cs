using System;
using System.Collections.Generic;
using System.Linq;
using Domains.Items;
using Domains.Items.Events;
using Domains.Scripts;
using Domains.UI;
using Gameplay.Events;
using JetBrains.Annotations;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Scene.Scripts
{
    public class PlayerInventoryManager : MonoBehaviour, MMEventListener<InventoryEvent>, MMEventListener<ItemEvent>
    {
        private const string InventoryKey = "InventoryContent";
        private const string WeightLimitKey = "InventoryMaxWeight";
        private const string ResourcesPath = "Items";

        // Weight-related properties
        [FormerlySerializedAs("_weightLimit")] [SerializeField]
        private float weightLimit = 100f; // Default value

        // UI updater reference
        [CanBeNull] public InventoryBarUpdater inventoryBarUpdater;

        // Direct reference to the inventory
        [FormerlySerializedAs("PlayerInventory")]
        public Inventory playerInventory;

        private string _savePath;

        // Single instance for easy access
        public static PlayerInventoryManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
                Instance = this;
            else if (Instance != this) Destroy(gameObject);
        }

        private void Start()
        {
            // Find the inventory
            playerInventory = FindFirstObjectByType<Inventory>();

            if (playerInventory == null)
            {
                UnityEngine.Debug.LogError("Failed to find Inventory component in scene!");
                return;
            }

            // Initialize the save path
            _savePath = SaveManager.SaveFileName;

            // Load or initialize inventory
            if (!ES3.FileExists(_savePath))
            {
                UnityEngine.Debug.Log("[PlayerInventoryManager] No save file found, initializing with defaults...");
                weightLimit = PlayerInfoSheet.WeightLimit;
                SaveInventory();
            }
            else
            {
                LoadInventory();
            }

            // Initialize UI if available
            if (inventoryBarUpdater != null) inventoryBarUpdater.Initialize();
        }

        private void OnEnable()
        {
            this.MMEventStartListening<ItemEvent>();
            this.MMEventStartListening<InventoryEvent>();
        }

        private void OnDisable()
        {
            this.MMEventStopListening<ItemEvent>();
            this.MMEventStopListening<InventoryEvent>();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath);
        }

        [Serializable]
        public class InventoryEntryData
        {
            [FormerlySerializedAs("UniqueID")] public string uniqueID;
            [FormerlySerializedAs("ItemID")] public string itemID;

            public InventoryEntryData(string uniqueID, string itemID)
            {
                this.uniqueID = uniqueID;
                this.itemID = itemID;
            }
        }

        #region Event Handling

        public void OnMMEvent(InventoryEvent eventType)
        {
            switch (eventType.EventType)
            {
                case InventoryEventType.ContentChanged:
                    SaveInventory();
                    break;

                case InventoryEventType.SellAllItems:
                    playerInventory.SellAllItems();
                    SaveInventory();
                    break;

                case InventoryEventType.UpgradedWeightLimit:
                    IncreaseWeightLimit(eventType.WeightLimitIncrease);
                    break;
            }
        }

        public void OnMMEvent(ItemEvent eventType)
        {
            if (eventType.EventType == ItemEventType.Picked)
            {
                UnityEngine.Debug.Log($"Item added to inventory: {eventType.Item.BaseItem.ItemName}");
                SaveInventory();
            }
        }

        #endregion

        #region Inventory Operations

        public bool AddItem(Inventory.InventoryEntry item)
        {
            // Check weight limit
            if (GetCurrentWeight() + item.BaseItem.ItemWeight > weightLimit)
            {
                playerInventory.inventoryFullFeedbacks?.PlayFeedbacks();
                UnityEngine.Debug.LogWarning("Inventory is full (weight limit reached)");
                return false;
            }

            // Add to inventory
            playerInventory.content.Add(item);

            // Trigger event
            InventoryEvent.Trigger(InventoryEventType.ContentChanged, playerInventory, 0);

            return true;
        }

        public bool RemoveItem(string uniqueID)
        {
            var item = playerInventory.content.Find(i => i.uniqueID == uniqueID);
            if (item == null)
            {
                UnityEngine.Debug.LogWarning($"Item with ID {uniqueID} not found in inventory");
                return false;
            }

            playerInventory.content.Remove(item);
            InventoryEvent.Trigger(InventoryEventType.ContentChanged, playerInventory, 0);

            return true;
        }

        public Inventory.InventoryEntry GetItem(string uniqueID)
        {
            return playerInventory.content.Find(i => i.uniqueID == uniqueID);
        }

        public void ClearInventory()
        {
            playerInventory.content.Clear();
            InventoryEvent.Trigger(InventoryEventType.ContentChanged, playerInventory, 0);
        }

        #endregion

        #region Save & Load

        public void SaveInventory()
        {
            if (!Application.isPlaying)
            {
                UnityEngine.Debug.Log("SaveInventory skipped in Editor mode");
                return;
            }

            // Save inventory entries
            var inventoryData = playerInventory.content.Select(entry =>
                new InventoryEntryData(entry.uniqueID, entry.BaseItem.ItemID)).ToList();

            ES3.Save(InventoryKey, inventoryData, _savePath);

            // Save weight limit
            ES3.Save(WeightLimitKey, weightLimit, _savePath);

            UnityEngine.Debug.Log($"✅ Saved inventory data to {_savePath}");
        }

        public void LoadInventory()
        {
            if (ES3.FileExists(_savePath))
            {
                // Load weight limit
                if (ES3.KeyExists(WeightLimitKey, _savePath)) weightLimit = ES3.Load<float>(WeightLimitKey, _savePath);

                // Load inventory content
                if (ES3.KeyExists(InventoryKey, _savePath))
                {
                    var inventoryData = ES3.Load<List<InventoryEntryData>>(InventoryKey, _savePath);

                    // Clear current inventory
                    playerInventory.content.Clear();

                    // Populate inventory
                    foreach (var itemData in inventoryData)
                    {
                        var baseItem = GetItemByID(itemData.itemID);
                        if (baseItem != null)
                        {
                            var entry = new Inventory.InventoryEntry(itemData.uniqueID, baseItem);
                            playerInventory.content.Add(entry);
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning($"Could not load item with ID {itemData.itemID}");
                        }
                    }

                    // Update UI
                    InventoryEvent.Trigger(InventoryEventType.ContentChanged, playerInventory, weightLimit);

                    UnityEngine.Debug.Log($"✅ Loaded inventory data from {_savePath}");
                }
            }
            else
            {
                UnityEngine.Debug.Log($"No saved inventory data found at {_savePath}. Using defaults.");
                playerInventory.content.Clear();
                weightLimit = PlayerInfoSheet.WeightLimit;
            }
        }

        private static BaseItem GetItemByID(string itemID)
        {
            return Resources.LoadAll<BaseItem>(ResourcesPath).FirstOrDefault(i => i.ItemID == itemID);
        }

        public static void ResetInventory()
        {
            if (Instance != null)
            {
                Instance.ClearInventory();
                Instance.weightLimit = PlayerInfoSheet.WeightLimit;
                Instance.SaveInventory();
            }
            else if (!Application.isPlaying)
            {
                // Edge case for when called from Editor
                var saveFilePath = SaveManager.SaveFileName;

                if (ES3.FileExists(saveFilePath))
                {
                    if (ES3.KeyExists(InventoryKey, saveFilePath)) ES3.DeleteKey(InventoryKey, saveFilePath);

                    if (ES3.KeyExists(WeightLimitKey, saveFilePath)) ES3.DeleteKey(WeightLimitKey, saveFilePath);

                    UnityEngine.Debug.Log($"Deleted inventory data from {saveFilePath}");
                }
            }
        }

        #endregion

        #region Weight Management

        public float GetMaxWeight()
        {
            return weightLimit;
        }

        public float GetCurrentWeight()
        {
            return playerInventory.content.Sum(entry => entry.BaseItem.ItemWeight);
        }

        public void IncreaseWeightLimit(float amount)
        {
            if (float.IsInfinity(weightLimit) || float.IsNaN(amount))
                return;

            weightLimit += amount;
            SaveInventory();

            InventoryEvent.Trigger(InventoryEventType.ContentChanged, playerInventory, weightLimit);
        }

        public void SetWeightLimit(float newLimit)
        {
            weightLimit = newLimit;
            SaveInventory();
        }

        #endregion
    }
}