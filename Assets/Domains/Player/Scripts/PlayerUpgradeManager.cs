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
        // ---------------------------------------------------------
        // 1) Static Fields: actual data you want to keep globally
        // ------------------------------------------------
        private static readonly Dictionary<string, int> UpgradeLevels = new();

        private static float shovelToolEffectRadius = 0.8f;
        private static float shovelToolEffectOpacity = 10f;

        // Literally the width of the onscreen tool
        private static float miningToolWidth = 0.6410909f; // Default mining tool width
        private static float fuelCapacity = 100f; // Default fuel capacity

        private static string currentToolId = "Shovel"; // Default starting tool

        // ---------------------------------------------------------
        // 2) Instance Fields: references to scene objects
        // --------------------------------------------
        [SerializeField] private List<UpgradeData> availableUpgrades;

        public MMFeedbacks upgradeFeedback;
        [SerializeField] private GameObject miningTool;
        private CharacterStatProfile characterStatProfile;
        private ShovelMiningState shovelMiningState;

        private void Awake()
        {
            characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            // Find ShovelMiningState if not assigned
            if (shovelMiningState == null)
            {
                shovelMiningState = FindFirstObjectByType<ShovelMiningState>();
                if (shovelMiningState == null)
                    UnityEngine.Debug.LogWarning(
                        "ShovelMiningState not found. Mining upgrades may not apply correctly.");
            }


            if (characterStatProfile != null)
            {
                shovelToolEffectRadius = characterStatProfile.initialShovelToolEffectRadius;
                shovelToolEffectOpacity = characterStatProfile.initialShovelToolEffectOpacity;
                // Literally the width of the onscreen tool
                miningToolWidth = characterStatProfile.MiningToolWidth; // Use your default value here
            }
            else
            {
                UnityEngine.Debug.LogError("CharacterStatProfile not set in PlayerStaminaManager");
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
            if (eventType.EventType == UpgradeEventType.UpgradePurchased)
                // We don't need to call BuyUpgrade again here since it would cause a loop
                // The event is just to notify other components that an upgrade was purchased
                // Instead, maybe log the event
                UnityEngine.Debug.Log($"Received UpgradePurchased event for {eventType.UpgradeData.upgradeTypeName}");
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
            var secondaryEffectType = upgrade.secondaryEffectTypes[level];
            var secondaryEffectValue = upgrade.secondaryEffectValues[level];

            switch (effectType)
            {
                case UpgradeEffectType.Multiplier:
                    ApplyMultiplierUpgrade(upgrade.upgradeTypeName, effectValue, secondaryEffectValue);
                    break;
                case UpgradeEffectType.Addition:
                    ApplyAdditionUpgrade(upgrade.upgradeTypeName, effectValue);
                    break;
                case UpgradeEffectType.ToolChange:
                    ApplyToolChangeUpgrade(toolId);
                    break;
            }
        }

        private void ApplyMultiplierUpgrade(string upgradeType, float multiplier, float secondaryMultiplier = 1)
        {
            UnityEngine.Debug.Log($"Applying multiplier upgrade: x{multiplier} to {upgradeType}");

            if (upgradeType == "Endurance") // Example: Multiply stamina
            {
                var newFuel = PlayerFuelManager.MaxFuelPoints * multiplier;
                PlayerFuelManager.MaxFuelPoints = newFuel;

                FuelEvent.Trigger(FuelEventType.SetMaxFuel, newFuel, newFuel);
            }
            else if (upgradeType == "Mining") // Example: Multiply mining speed
            {
                // Calculate new size
                var newSize = shovelMiningState.GetSize() * multiplier;
                var newWidth = miningToolWidth * multiplier;
                var newOpacity = shovelToolEffectOpacity * secondaryMultiplier;

                // Clamp the value
                newWidth = Mathf.Clamp(newWidth, 1f, 2f);

                // Apply the clamped size
                if (shovelMiningState != null)
                {
                    shovelMiningState.SetShovelEffectSize(newSize, newOpacity);
                    shovelToolEffectRadius = newSize;
                    var oldScale = miningTool.transform.localScale;
                    miningTool.transform.localScale = new Vector3(newWidth, oldScale.y, oldScale.z);
                    miningToolWidth = newWidth; // Update the width as well
                }
                else
                {
                    UnityEngine.Debug.LogWarning("ShovelMiningState reference is null during multiplier upgrade");
                }


                // Log the size change for debugging
                UnityEngine.Debug.Log($"Mining size changed to: {shovelToolEffectRadius}");

                // Save immediately
                ES3.Save("MiningToolSize", shovelToolEffectRadius, "UpgradeSave.es3");
                ES3.Save("MiningToolOpacity", shovelToolEffectOpacity, "UpgradeSave.es3");
                ES3.Save("MiningToolWidth", shovelToolEffectRadius, "UpgradeSave.es3");
            }
        }

        private void ApplyAdditionUpgrade(string upgradeType, float addition)
        {
            UnityEngine.Debug.Log($"Applying addition upgrade: +{addition} to {upgradeType}");

            if (upgradeType == "Inventory")
            {
                PlayerInventoryManager.IncreaseWeightLimit(addition);
                InventoryEvent.Trigger(InventoryEventType.UpgradedWeightLimit, PlayerInventoryManager.PlayerInventory,
                    addition);
            }
            else if (upgradeType == "Endurance")
            {
                // Increase stamina directly
                PlayerFuelManager.MaxFuelPoints += addition;
                ES3.Save("MaxStamina", PlayerFuelManager.MaxFuelPoints, "UpgradeSave.es3"); // Save stamina
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
            foreach (var upgrade in UpgradeLevels)
                ES3.Save(upgrade.Key, upgrade.Value, "UpgradeSave.es3");

            // Save mining tool size
            ES3.Save("MiningToolSize", shovelToolEffectRadius, "UpgradeSave.es3");
            ES3.Save("MiningToolOpacity", shovelToolEffectOpacity, "UpgradeSave.es3");
            ES3.Save("MiningToolWidth", miningToolWidth, "UpgradeSave.es3");

            // Save current tool ID
            ES3.Save("CurrentToolID", currentToolId, "UpgradeSave.es3");

            // Save stamina
            ES3.Save("MaxStamina", PlayerFuelManager.MaxFuelPoints, "UpgradeSave.es3");

            // Save fuel capacity
            ES3.Save("MaxFuelCapacity", fuelCapacity, "UpgradeSave.es3");

            ES3.Save("InventoryMaxWeight", PlayerInventoryManager.GetMaxWeight(), "GameSave.es3");
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
            UnityEngine.Debug.Log("Loading upgrades...");

            // Load upgrade levels
            foreach (var upgrade in availableUpgrades)
                if (ES3.KeyExists(upgrade.upgradeTypeName, "UpgradeSave.es3"))
                    UpgradeLevels[upgrade.upgradeTypeName] = ES3.Load<int>(upgrade.upgradeTypeName, "UpgradeSave.es3");
                else
                    UpgradeLevels[upgrade.upgradeTypeName] = 0;

            // Load saved values directly without re-applying effects

            // Load mining tool size and apply directly
            if (ES3.KeyExists("MiningToolSize", "UpgradeSave.es3") &&
                ES3.KeyExists("MiningToolOpacity", "UpgradeSave.es3"))
            {
                shovelToolEffectRadius = ES3.Load<float>("MiningToolSize", "UpgradeSave.es3");
                shovelToolEffectOpacity =
                    ES3.Load<float>("MiningToolOpacity", "UpgradeSave.es3");

                // Directly update the ShovelMiningState
                if (shovelMiningState != null)
                {
                    shovelMiningState.SetShovelEffectSize(shovelToolEffectRadius, shovelToolEffectOpacity);
                    UnityEngine.Debug.Log($"Setting shovel mining size to {shovelToolEffectRadius}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("ShovelMiningState reference is null during LoadUpgrades");
                }
            }

            // Load mining tool width and apply directly
            if (ES3.KeyExists("MiningToolWidth", "UpgradeSave.es3"))
            {
                miningToolWidth = ES3.Load<float>("MiningToolWidth", "UpgradeSave.es3");

                var oldScale = miningTool.transform.localScale;
                miningTool.transform.localScale = new Vector3(miningToolWidth, oldScale.y, oldScale.z);
            }

            // Load stamina
            if (ES3.KeyExists("MaxStamina", "UpgradeSave.es3"))
                PlayerFuelManager.MaxFuelPoints = ES3.Load<float>("MaxStamina", "UpgradeSave.es3");

            // Load fuel capacity
            if (ES3.KeyExists("MaxFuelCapacity", "UpgradeSave.es3"))
                fuelCapacity = ES3.Load<float>("MaxFuelCapacity", "UpgradeSave.es3");

            // Load inventory size
            if (ES3.KeyExists("InventoryMaxWeight", "GameSave.es3"))
            {
                var savedWeight = ES3.Load<float>("InventoryMaxWeight", "GameSave.es3");
                PlayerInventoryManager.SetWeightLimit(savedWeight);
            }

            // Load tool ID if necessary
            if (ES3.KeyExists("CurrentToolID", "UpgradeSave.es3"))
                currentToolId = ES3.Load<string>("CurrentToolID", "UpgradeSave.es3");

            UnityEngine.Debug.Log("Finished loading all upgrades");
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

            // Reset mining tool size to default value

            shovelToolEffectRadius = characterStatProfile.initialShovelToolEffectRadius; // Use your default value here
            miningToolWidth = characterStatProfile.MiningToolWidth; // Use your default value here
            shovelToolEffectOpacity =
                characterStatProfile.initialShovelToolEffectOpacity; // Use your default value here

            fuelCapacity = characterStatProfile.InitialMaxFuel;


            UpgradeEvent.Trigger(UpgradeType.Mining, UpgradeEventType.ShovelMiningSizeSet, null, 0,
                UpgradeEffectType.None, shovelToolEffectRadius, null, shovelToolEffectOpacity);
        }
    }
}