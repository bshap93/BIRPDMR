using System;
using Domains.Debug;
using Domains.Player.Events;
using Domains.Player.Scripts;
using Domains.Player.Scripts.ScriptableObjects;
using Domains.SaveLoad;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Scene.Scripts
{
    [Serializable]
    public class SaveManager : MonoBehaviour, MMEventListener<SaveLoadEvent>
    {
        public const string SaveFileName = "GameSave.es3";
        public const string SavePickablesFileName = "Pickables.es3";

        // [Header("Persistence Managers")] [SerializeField]
        // InventoryPersistenceManager inventoryManager;
        [FormerlySerializedAs("playerMutableStatsManager")]
        [FormerlySerializedAs("playerStatsManager")]
        [FormerlySerializedAs("resourcesManager")]
        [SerializeField]
        private PlayerStaminaManager playerStaminaManager;

        [SerializeField] private PlayerHealthManager playerHealthManager;

        [Header("Item & Container Persistence")]
        public PickableManager pickableManager;

        public PlayerInventoryManager playerInventoryManager;

        public PlayerCurrencyManager playerCurrencyManager;

        public PlayerUpgradeManager playerUpgradeManager;


        public static SaveManager Instance { get; private set; }

        public bool freshStart;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (freshStart) DataReset.ClearAllSaveData();

            // Initialize managers if needed
            if (pickableManager == null)
            {
                pickableManager = GetComponentInChildren<PickableManager>(true);
                if (pickableManager == null)
                {
                    var pickableGO = new GameObject("PickableManager");
                    pickableManager = pickableGO.AddComponent<PickableManager>();
                    pickableGO.transform.SetParent(transform);
                }
            }

            if (playerInventoryManager == null)
            {
                playerInventoryManager = GetComponentInChildren<PlayerInventoryManager>(true);
                if (playerInventoryManager == null)
                {
                    var inventoryGO = new GameObject("PlayerInventoryManager");
                    playerInventoryManager = inventoryGO.AddComponent<PlayerInventoryManager>();
                    inventoryGO.transform.SetParent(transform);
                }
            }


            if (playerStaminaManager == null)
            {
                playerStaminaManager = GetComponentInChildren<PlayerStaminaManager>(true);
                if (playerStaminaManager == null)
                    UnityEngine.Debug.LogError("PlayerStaminaManager not found in SaveManager");
            }

            if (playerHealthManager == null)
            {
                playerHealthManager = GetComponentInChildren<PlayerHealthManager>(true);
                if (playerHealthManager == null)
                    UnityEngine.Debug.LogError("PlayerHealthManager not found in SaveManager");
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

        public void OnMMEvent(SaveLoadEvent eventType)
        {
            if (eventType.EventType == SaveLoadEventType.Save)
                SaveAll();
            else if (eventType.EventType == SaveLoadEventType.Load) LoadAll();
        }


        private string GetSaveFileName()
        {
            return "GameSave.es3"; // Always use a single save file
        }

        public void SaveAll()
        {
            PlayerStaminaManager.SavePlayerStamina();
            PlayerHealthManager.SavePlayerHealth();
            PlayerInventoryManager.SaveInventory();
            PlayerCurrencyManager.SavePlayerCurrency();
            PlayerUpgradeManager.SaveUpgrades();
            PickableManager.SaveAllPickedItems();
            DiggerEvent.Trigger(DiggerEventType.Persist);
            UnityEngine.Debug.Log("All data saved");
        }

        public bool LoadAll()
        {
            var staminaLoaded = playerStaminaManager != null && playerStaminaManager.HasSavedData();
            var healthLoaded = playerHealthManager != null && playerHealthManager.HasSavedData();
            var inventoryLoaded = playerInventoryManager != null && playerInventoryManager.HasSavedData();
            var currencyLoaded = playerCurrencyManager != null && playerCurrencyManager.HasSavedData();
            var upgradesLoaded = playerUpgradeManager != null && playerUpgradeManager.HasSavedData();
            var pickablesLoaded = pickableManager != null && pickableManager.HasSavedData();

            // Digger has no Load method


            if (staminaLoaded) playerStaminaManager.LoadPlayerStamina();
            if (healthLoaded) playerHealthManager.LoadPlayerHealth();
            if (inventoryLoaded) playerInventoryManager.LoadInventory();
            if (currencyLoaded) playerCurrencyManager.LoadPlayerCurrency();
            if (upgradesLoaded) playerUpgradeManager.LoadUpgrades();
            if (pickablesLoaded) pickableManager.LoadPickedItems();


            return staminaLoaded ||
                   healthLoaded || inventoryLoaded || currencyLoaded ||
                   upgradesLoaded;
        }


        // On App Quit
        private void OnApplicationQuit()
        {
            SaveAll();
        }
    }
}