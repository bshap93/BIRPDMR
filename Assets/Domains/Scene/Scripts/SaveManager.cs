using System;
using System.Collections;
using Domains.Debug;
using Domains.Player.Events;
using Domains.Player.Scripts;
using Domains.SaveLoad;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
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

        [SerializeField] private MMFeedbacks saveFeedbacks;

        [Header("Item & Container Persistence")]
        public PickableManager pickableManager;

        public PlayerInventoryManager playerInventoryManager;

        public PlayerCurrencyManager playerCurrencyManager;

        public PlayerUpgradeManager playerUpgradeManager;

        public DestructableManager destructableManager;

        public bool freshStart;


        public static SaveManager Instance { get; private set; }

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
                    var pickableGo = new GameObject("PickableManager");
                    pickableManager = pickableGo.AddComponent<PickableManager>();
                    pickableGo.transform.SetParent(transform);
                }
            }

            if (playerInventoryManager == null)
            {
                playerInventoryManager = GetComponentInChildren<PlayerInventoryManager>(true);
                if (playerInventoryManager == null)
                {
                    var inventoryGo = new GameObject("PlayerInventoryManager");
                    playerInventoryManager = inventoryGo.AddComponent<PlayerInventoryManager>();
                    inventoryGo.transform.SetParent(transform);
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


        // On App Quit
        private void OnApplicationQuit()
        {
            StartCoroutine(SaveAllThenWait());
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
            if (playerInventoryManager != null)
                PlayerInventoryManager.Instance.SaveInventory();
            else
                UnityEngine.Debug.LogError("PlayerInventoryManager.Instance is null. Skipping inventory save.");
            PlayerCurrencyManager.SavePlayerCurrency();
            PlayerUpgradeManager.SaveUpgrades();
            PickableManager.SaveAllPickedItems();
            DestructableManager.SaveAllDestructables();
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
            var destructablesLoaded = destructableManager != null && destructableManager.HasSavedData();

            // Digger has no Load method


            if (staminaLoaded) playerStaminaManager.LoadPlayerStamina();
            if (healthLoaded) playerHealthManager.LoadPlayerHealth();
            if (inventoryLoaded) playerInventoryManager.LoadInventory();
            if (currencyLoaded) playerCurrencyManager.LoadPlayerCurrency();
            if (upgradesLoaded) playerUpgradeManager.LoadUpgrades();
            if (pickablesLoaded) pickableManager.LoadPickedItems();
            if (destructablesLoaded) destructableManager.LoadDestructables();


            return staminaLoaded ||
                   healthLoaded || inventoryLoaded || currencyLoaded ||
                   upgradesLoaded || pickablesLoaded || destructablesLoaded;
        }

        public void CallSaveThenWait()
        {
            StartCoroutine(SaveAllThenWait());
        }

        private IEnumerator SaveAllThenWait()
        {
            SaveAll();
            saveFeedbacks?.PlayFeedbacks();
            AlertEvent.Trigger(AlertType.SavingGame, "Saving game...");
            yield return new WaitForSeconds(1);
        }
    }
}