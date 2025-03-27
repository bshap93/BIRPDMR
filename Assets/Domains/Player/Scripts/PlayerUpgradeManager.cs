using System.Collections.Generic;
using Domains.Items.Events;
using Domains.Player.Events;
using Domains.Player.Scripts.ScriptableObjects;
using Domains.Scene.Scripts;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public class PlayerUpgradeManager : MonoBehaviour, MMEventListener<UpgradeEvent>
    {
        private static PlayerUpgradeManager _instance;
        private static readonly Dictionary<string, int> UpgradeLevels = new();
        [SerializeField] private List<UpgradeData> availableUpgrades;
        [SerializeField] private ShovelMiningState shovelMiningState;

        public MMFeedbacks upgradeFeedback;

        [SerializeField] private string currentToolId = "Shovel"; // Default starting tool


        public float miningToolSize = 0.2f;


        [SerializeField] private float fuelCapacity = 100f; // Default fuel capacity

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                UnityEngine.Debug.LogWarning("Duplicate PlayerUpgradeManager detected. Destroying the extra instance.");
                Destroy(gameObject);
            }
        }


        private void Start()
        {
            LoadUpgrades();

        }


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(UpgradeEvent eventType)
        {
            // if (eventType.EventType == UpgradeEventType.UpgradePurchased)
            // {
            //     BuyUpgrade(eventType.UpgradeData.upgradeTypeName);
            // }
        }


        private void ApplyToolChangeUpgrade(string toolId)
        {
            if (string.IsNullOrEmpty(toolId))
            {
                UnityEngine.Debug.LogWarning("Tool ID is empty; skipping tool change upgrade.");
                return;
            }

            UnityEngine.Debug.Log($"Changing tool to {toolId}");

            // Store the tool ID
            currentToolId = toolId;

            // Save it
            ES3.Save("CurrentToolID", currentToolId, "UpgradeSave.es3");
        }

        public void BuyUpgrade(string upgradeTypeName)
        {
            if (!UpgradeLevels.ContainsKey(upgradeTypeName))
                UpgradeLevels[upgradeTypeName] = 0;

            var currentLevel = UpgradeLevels[upgradeTypeName];
            var upgrade = availableUpgrades.Find(u => u.upgradeTypeName == upgradeTypeName);

            if (upgrade == null || currentLevel >= upgrade.upgradeCosts.Length)
            {
                UnityEngine.Debug.Log("Max Level Reached");
                return;
            }

            var cost = upgrade.upgradeCosts[currentLevel];

            var upgradeType = UpgradeType.None;
            if (upgradeTypeName == "Mining") upgradeType = UpgradeType.Mining;
            else if (upgradeTypeName == "Endurance") upgradeType = UpgradeType.Endurance;
            else if (upgradeTypeName == "Inventory") upgradeType = UpgradeType.Inventory;

            if (PlayerCurrencyManager.CompanyCredits >= cost)
            {
                CurrencyEvent.Trigger(CurrencyEventType.RemoveCurrency, cost);
                UpgradeLevels[upgradeTypeName]++;
                SaveUpgrades();

                // Play feedbacks
                upgradeFeedback?.PlayFeedbacks();

                // Apply Upgrade Effect
                ApplyUpgradeEffect(upgrade, currentLevel);

                // Trigger event
                UpgradeEvent.Trigger(
                    upgradeType,
                    UpgradeEventType.UpgradePurchased,
                    upgrade,
                    UpgradeLevels[upgradeTypeName],
                    upgrade.effectTypes[currentLevel],
                    upgrade.effectValues[currentLevel],
                    upgrade.effectTypes[currentLevel] == UpgradeEffectType.ToolChange
                        ? upgrade.toolChangeIDs[currentLevel]
                        : null
                );

                UpdateUI();
            }
            else
            {
                UpgradeEvent.Trigger(upgradeType, UpgradeEventType.UpgradeFailed, upgrade,
                    UpgradeLevels[upgradeTypeName], UpgradeEffectType.None, 0);
                UnityEngine.Debug.Log("Not enough credits!");
            }
        }

        private void ApplyUpgradeEffect(UpgradeData upgrade, int level)
        {
            var effectType = upgrade.effectTypes[level];
            var effectValue = upgrade.effectValues[level];
            var toolId = effectType == UpgradeEffectType.ToolChange ? upgrade.toolChangeIDs[level] : null;

            switch (effectType)
            {
                case UpgradeEffectType.Multiplier:
                    ApplyMultiplierUpgrade(upgrade.upgradeTypeName, effectValue);
                    break;
                case UpgradeEffectType.Addition:
                    ApplyAdditionUpgrade(upgrade.upgradeTypeName, effectValue);
                    break;
                case UpgradeEffectType.ToolChange:
                    ApplyToolChangeUpgrade(toolId);
                    break;
            }
        }

        private void ApplyMultiplierUpgrade(string upgradeType, float multiplier)
        {
            UnityEngine.Debug.Log($"Applying multiplier upgrade: x{multiplier} to {upgradeType}");

            if (upgradeType == "Endurance") // Example: Multiply stamina
            {
                var newStamina = PlayerStaminaManager.MaxStaminaPoints * multiplier;
                PlayerStaminaManager.MaxStaminaPoints = newStamina;

                StaminaEvent.Trigger(StaminaEventType.SetMaxStamina, newStamina);
            }
            else if (upgradeType == "Mining") // Example: Multiply mining speed
            {
                shovelMiningState.size *= multiplier;
            }
        }

        private void ApplyAdditionUpgrade(string upgradeType, float addition)
        {
            UnityEngine.Debug.Log($"Applying addition upgrade: +{addition} to {upgradeType}");

            if (upgradeType == "Inventory")
            {
                var inventoryManager = PlayerInventoryManager.Instance;
                if (inventoryManager != null)
                {
                    inventoryManager.IncreaseWeightLimit(addition);
                    InventoryEvent.Trigger(InventoryEventType.UpgradedWeightLimit, inventoryManager.PlayerInventory,
                        addition);
                }
            }
            else if (upgradeType == "Endurance")
            {
                // Increase stamina directly
                PlayerStaminaManager.MaxStaminaPoints += addition;
                ES3.Save("MaxStamina", PlayerStaminaManager.MaxStaminaPoints, "UpgradeSave.es3"); // Save stamina
            }
            else if (upgradeType == "FuelCapacity")
            {
                // Increase fuel capacity
                fuelCapacity += addition;
                ES3.Save("MaxFuelCapacity", fuelCapacity, "UpgradeSave.es3"); // Save fuel capacity
            }
        }

        private void UpdateUI()
        {
            // Update UI
        }

        public static void SaveUpgrades()
        {
            if (_instance == null)
            {
                UnityEngine.Debug.LogWarning("PlayerUpgradeManager.Instance is null. Skipping SaveUpgrades.");
                return;
            }

            foreach (var upgrade in UpgradeLevels)
                ES3.Save(upgrade.Key, upgrade.Value, "UpgradeSave.es3");

            // Save mining tool size
            ES3.Save("MiningToolSize", _instance.miningToolSize, "UpgradeSave.es3");

            // Save current tool ID
            ES3.Save("CurrentToolID", _instance.currentToolId, "UpgradeSave.es3");

            // Save stamina
            ES3.Save("MaxStamina", PlayerStaminaManager.MaxStaminaPoints, "UpgradeSave.es3");

            // Save fuel capacity
            ES3.Save("MaxFuelCapacity", _instance.fuelCapacity, "UpgradeSave.es3");

            ES3.Save("InventoryMaxWeight", PlayerInventoryManager.Instance.GetMaxWeight(), "GameSave.es3");
        }


        public int GetUpgradeLevel(string upgradeName)
        {
            return UpgradeLevels.ContainsKey(upgradeName) ? UpgradeLevels[upgradeName] : 0;
        }

        public int GetUpgradeCost(string upgradeTypeName)
        {
            var level = GetUpgradeLevel(upgradeTypeName);
            return availableUpgrades.Find(u => u.upgradeTypeName == upgradeTypeName)?.upgradeCosts[level] ?? 9999;
        }

        public void LoadUpgrades()
        {
            foreach (var upgrade in availableUpgrades)
                if (ES3.KeyExists(upgrade.upgradeTypeName, "UpgradeSave.es3"))
                    UpgradeLevels[upgrade.upgradeTypeName] = ES3.Load<int>(upgrade.upgradeTypeName, "UpgradeSave.es3");
                else
                    UpgradeLevels[upgrade.upgradeTypeName] = 0;

            // Load mining tool size
            if (ES3.KeyExists("MiningToolSize", "UpgradeSave.es3"))
                miningToolSize = ES3.Load<float>("MiningToolSize", "UpgradeSave.es3");

            // Load current tool
            if (ES3.KeyExists("CurrentToolID", "UpgradeSave.es3"))
                currentToolId = ES3.Load<string>("CurrentToolID", "UpgradeSave.es3");

            // Load stamina
            if (ES3.KeyExists("MaxStamina", "UpgradeSave.es3"))
                PlayerStaminaManager.MaxStaminaPoints = ES3.Load<float>("MaxStamina", "UpgradeSave.es3");

            // Load fuel capacity
            if (ES3.KeyExists("MaxFuelCapacity", "UpgradeSave.es3"))
                fuelCapacity = ES3.Load<float>("MaxFuelCapacity", "UpgradeSave.es3");


            // Load inventory size
            if (ES3.KeyExists("InventoryMaxWeight", "GameSave.es3"))
            {
                var savedWeight = ES3.Load<float>("InventoryMaxWeight", "GameSave.es3");
                PlayerInventoryManager.Instance.SetWeightLimit(savedWeight); // You’ll need to implement this
            }

            foreach (var upgrade in availableUpgrades)
            {
                var level = GetUpgradeLevel(upgrade.upgradeTypeName);
                for (var i = 0; i < level; i++) // Apply all past levels
                {
                    if (upgrade.upgradeTypeName == "Inventory") continue;
                    ApplyUpgradeEffect(upgrade, i);
                    UnityEngine.Debug.Log($"Applying upgrade {upgrade.upgradeTypeName} level {i}");
                }
            }
        }


        public string GetUpgradeName(string upgradeTypeName)
        {
            var level = GetUpgradeLevel(upgradeTypeName);
            var upgrade = availableUpgrades.Find(u => u.upgradeTypeName == upgradeTypeName);

            if (upgrade != null && level < upgrade.upgradeNames.Length)
                return upgrade.upgradeNames[level]; // Return the name for the current level

            return "Unknown Upgrade"; // Default fallback
        }

        public bool HasSavedData()
        {
            return ES3.FileExists("UpgradeSave.es3");
        }

        public static void ResetPlayerUpgrades()
        {
            var characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            if (characterStatProfile == null)
            {
                UnityEngine.Debug.LogError("CharacterStatProfile not found! Using default values.");

                // 🔥 FIX: Store keys in a separate list before modifying dictionary
                var upgradeKeys = new List<string>(UpgradeLevels.Keys);
                foreach (var key in upgradeKeys) UpgradeLevels[key] = 0;
            }
            else
            {
                // 🔥 FIX: Same approach, storing keys separately
                var upgradeKeys = new List<string>(UpgradeLevels.Keys);
                foreach (var key in upgradeKeys) UpgradeLevels[key] = characterStatProfile.InitialUpgradeState;
            }
        }
    }
}