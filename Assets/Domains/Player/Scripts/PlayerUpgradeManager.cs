using System.Collections.Generic;
using Domains.Gameplay.Equipment.Scripts;
using Domains.Gameplay.Tools.ToolSpecifics;
using Domains.Items.Events;
using Domains.Player.Events;
using Domains.Player.Scripts.ScriptableObjects;
using Domains.Scene.Scripts;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Player.Scripts
{
    [DefaultExecutionOrder(0)] // Make this run after DataReset
    public class PlayerUpgradeManager : MonoBehaviour, MMEventListener<UpgradeEvent>
    {
        // 1) Static Fields: actual data you want to keep globally
        // ------------------------------------------------
        private static readonly Dictionary<string, int> UpgradeLevels = new();

        // Tool effect properties
        private static float shovelToolEffectRadius = 0.8f;
        private static float shovelToolEffectOpacity = 10f;
        private static float pickaxeToolEffectRadius = 0.8f;
        private static float pickaxeToolEffectOpacity = 10f;

        // Tool dimensions
        private static float shovelToolWidth = 0.6410909f;
        private static float pickaxeMiningToolWidth = 0.6410909f;

        // Current material indices for tools
        private static int shovelMaterialLevel;
        private static int pickaxeMaterialLevel;

        private static float fuelCapacity = 100f;
        private static string currentToolId = "Shovel";

        // Tool references
        private static PickaxeTool pickaxeTool;
        private static ShovelTool shovelTool;

        // ---------------------------------------------------------
        // 2) Instance Fields: references to scene objects
        // --------------------------------------------
        [SerializeField] private List<UpgradeData> availableUpgrades;
        public MMFeedbacks upgradeFeedback;
        private CharacterStatProfile characterStatProfile;

        // Debug flags
        private bool isInitialLoad = true;

        private void Awake()
        {
            UnityEngine.Debug.Log("PlayerUpgradeManager.Awake() - Starting initialization");

            characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            if (characterStatProfile == null)
            {
                UnityEngine.Debug.LogError("CharacterStatProfile not found! Upgrades may not work correctly.");
                return;
            }

            // Find tools
            InitializeToolReferences();

            // Initialize default values from profile
            InitializeDefaultValues();


            UnityEngine.Debug.Log("PlayerUpgradeManager.Awake() - Initialization complete");
        }


        private void Start()
        {
            UnityEngine.Debug.Log("PlayerUpgradeManager.Start() - Loading upgrades");
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

        private void InitializeToolReferences()
        {
            if (shovelTool == null)
            {
                shovelTool = FindShovelTool();
                if (shovelTool == null)
                    UnityEngine.Debug.LogWarning("ShovelTool not found. Upgrades may not apply correctly.");
            }

            if (pickaxeTool == null)
            {
                pickaxeTool = FindPickaxeTool();
                if (pickaxeTool == null)
                    UnityEngine.Debug.LogWarning("PickaxeTool not found. Upgrades may not apply correctly.");
            }
        }

        private void InitializeDefaultValues()
        {
            // Set default values from character profile
            shovelToolEffectRadius = characterStatProfile.initialShovelToolEffectRadius;
            shovelToolEffectOpacity = characterStatProfile.initialShovelToolEffectOpacity;
            shovelToolWidth = characterStatProfile.shovelMiningToolWidth;

            pickaxeMiningToolWidth = characterStatProfile.pickaxeMiningToolWidth;
            pickaxeToolEffectRadius = characterStatProfile.initialPickaxeToolEffectRadius;
            pickaxeToolEffectOpacity = characterStatProfile.pickaxeMiningToolEffectOpacity;

            // Reset material levels (0 = initial/no upgrades)
            shovelMaterialLevel = 0;
            pickaxeMaterialLevel = 0;
        }

        private PickaxeTool FindPickaxeTool()
        {
            foreach (var tool in PlayerEquipment.Instance.Tools)
                if (tool is PickaxeTool pickaxe)
                    return pickaxe;

            UnityEngine.Debug.LogWarning("No PickaxeTool found in PlayerEquipment.");
            return null;
        }

        private ShovelTool FindShovelTool()
        {
            foreach (var tool in PlayerEquipment.Instance.Tools)
                if (tool is ShovelTool shovel)
                    return shovel;

            UnityEngine.Debug.LogWarning("No ShovelTool found in PlayerEquipment.");
            return null;
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

            var upgradeType = GetUpgradeTypeFromName(upgradeTypeName);


            if (PlayerCurrencyManager.CompanyCredits >= cost)
            {
                CurrencyEvent.Trigger(CurrencyEventType.RemoveCurrency, cost);
                UpgradeLevels[upgradeTypeName]++;

                // Apply Upgrade Effect
                ApplyUpgradeEffect(upgrade, currentLevel);
                SaveUpgrades();

                // Play feedbacks
                upgradeFeedback?.PlayFeedbacks();

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
                        : null,
                    upgrade.secondaryEffectValues[currentLevel],
                    upgrade.upgradeMaterials[currentLevel]
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

        private UpgradeType GetUpgradeTypeFromName(string upgradeTypeName)
        {
            switch (upgradeTypeName)
            {
                case "Shovel": return UpgradeType.Shovel;
                case "Pickaxe": return UpgradeType.Pickaxe;
                case "Endurance": return UpgradeType.Endurance;
                case "Inventory": return UpgradeType.Inventory;
                default: return UpgradeType.None;
            }
        }

        private void ApplyUpgradeEffect(UpgradeData upgrade, int level)
        {
            var effectType = upgrade.effectTypes[level];
            var effectValue = upgrade.effectValues[level];
            var toolId = effectType == UpgradeEffectType.ToolChange ? upgrade.toolChangeIDs[level] : null;
            var secondaryEffectType = upgrade.secondaryEffectTypes[level];
            var secondaryEffectValue = upgrade.secondaryEffectValues[level];
            var upgradeColor = upgrade.upgradeColors[level];
            var upgradeMaterial = upgrade.upgradeMaterials[level];

            switch (effectType)
            {
                case UpgradeEffectType.Multiplier:
                    ApplyMultiplierUpgrade(level, upgrade.upgradeTypeName, effectValue, secondaryEffectValue,
                        upgradeMaterial);
                    break;
                case UpgradeEffectType.Addition:
                    ApplyAdditionUpgrade(upgrade.upgradeTypeName, effectValue);
                    break;
                case UpgradeEffectType.ToolChange:
                    ApplyToolChangeUpgrade(toolId);
                    break;
            }
        }

        private void ApplyColorUpgrade(string upgradeType, Color color)
        {
            UnityEngine.Debug.Log($"Applying color upgrade: {color} to {upgradeType}");
        }

        private void ApplyMultiplierUpgrade(int level, string upgradeType, float multiplier,
            float secondaryMultiplier = 1, Material upgradeMaterial = null)
        {
            UnityEngine.Debug.Log($"Applying multiplier upgrade: x{multiplier} to {upgradeType}");

            switch (upgradeType)
            {
                case "Endurance":
                    ApplyEnduranceUpgrade(multiplier);
                    break;
                case "Shovel":
                    ApplyShovelUpgrade(level, multiplier, secondaryMultiplier, upgradeMaterial);
                    break;
                case "Pickaxe":
                    ApplyPickaxeUpgrade(level, multiplier, secondaryMultiplier, upgradeMaterial);
                    break;
            }
        }

        private void ApplyEnduranceUpgrade(float multiplier)
        {
            var newFuel = PlayerFuelManager.MaxFuelPoints * multiplier;
            PlayerFuelManager.MaxFuelPoints = newFuel;
            FuelEvent.Trigger(FuelEventType.SetMaxFuel, newFuel, newFuel);
        }

        private void ApplyShovelUpgrade(int level, float multiplier, float secondaryMultiplier,
            Material upgradeMaterial)
        {
            if (shovelTool == null) return;

            // Calculate new effects
            var newEffectRadius = shovelTool.effectRadius * multiplier;
            var newOpacity = shovelToolEffectOpacity * secondaryMultiplier;

            // Apply effects
            shovelTool.SetDiggerUsingToolEffectSize(newEffectRadius, newOpacity);
            shovelToolEffectRadius = newEffectRadius;
            shovelToolEffectOpacity = newOpacity;

            // Update material if provided
            if (upgradeMaterial != null)
            {
                shovelTool.SetCurrentMaterial(upgradeMaterial);
                shovelMaterialLevel = level;
                UnityEngine.Debug.Log($"Applied shovel material upgrade to level {level}");
            }

            UnityEngine.Debug.Log($"Shovel mining size changed to: {shovelToolEffectRadius}");
        }

        private void ApplyPickaxeUpgrade(int level, float multiplier, float secondaryMultiplier,
            Material upgradeMaterial)
        {
            if (pickaxeTool == null) return;

            // Calculate new effects
            var newEffectRadius = pickaxeTool.effectRadius * multiplier;
            var newWidth = pickaxeMiningToolWidth * multiplier;
            var newOpacity = pickaxeToolEffectOpacity * secondaryMultiplier;

            // Clamp width
            newWidth = Mathf.Clamp(newWidth, 1f, 2f);

            // Apply effects
            pickaxeTool.SetDiggerUsingToolEffectSize(newEffectRadius, newOpacity);
            pickaxeToolEffectRadius = newEffectRadius;
            pickaxeToolEffectOpacity = newOpacity;

            // Apply scale
            var oldScale = pickaxeTool.transform.localScale;
            pickaxeTool.transform.localScale = new Vector3(newWidth, oldScale.y, oldScale.z);
            pickaxeMiningToolWidth = newWidth;

            // Update material if provided
            if (upgradeMaterial != null)
            {
                pickaxeTool.SetCurrentMaterial(upgradeMaterial);
                pickaxeMaterialLevel = level;
                UnityEngine.Debug.Log($"Applied pickaxe material upgrade to level {level}: {upgradeMaterial.name}");

                // Explicitly save the material level right after applying it
                ES3.Save("PickaxeMaterialLevel", pickaxeMaterialLevel, "UpgradeSave.es3");
            }

            UnityEngine.Debug.Log($"Pickaxe mining size changed to: {pickaxeToolEffectRadius}");
        }

        private void ApplyAdditionUpgrade(string upgradeType, float addition)
        {
            UnityEngine.Debug.Log($"Applying addition upgrade: +{addition} to {upgradeType}");

            switch (upgradeType)
            {
                case "Inventory":
                    PlayerInventoryManager.IncreaseWeightLimit(addition);
                    InventoryEvent.Trigger(InventoryEventType.UpgradedWeightLimit,
                        PlayerInventoryManager.PlayerInventory, addition);
                    break;
                case "Endurance":
                    PlayerFuelManager.MaxFuelPoints += addition;
                    break;
                case "FuelCapacity":
                    fuelCapacity += addition;
                    break;
            }
        }


        private void UpdateUI()
        {
            // Update UI
        }

        public static void SaveUpgrades()
        {
            // Save upgrade levels
            foreach (var upgrade in UpgradeLevels)
                ES3.Save(upgrade.Key, upgrade.Value, "UpgradeSave.es3");

            // Save tool effects
            ES3.Save("ShovelToolEffectRadius", shovelToolEffectRadius, "UpgradeSave.es3");
            ES3.Save("ShovelToolEffectOpacity", shovelToolEffectOpacity, "UpgradeSave.es3");
            ES3.Save("ShovelToolWidth", shovelToolWidth, "UpgradeSave.es3");
            ES3.Save("ShovelMaterialLevel", shovelMaterialLevel, "UpgradeSave.es3");

            ES3.Save("PickaxeToolEffectRadius", pickaxeToolEffectRadius, "UpgradeSave.es3");
            ES3.Save("PickaxeToolEffectOpacity", pickaxeToolEffectOpacity, "UpgradeSave.es3");
            ES3.Save("PickaxeToolWidth", pickaxeMiningToolWidth, "UpgradeSave.es3");
            ES3.Save("PickaxeMaterialLevel", pickaxeMaterialLevel, "UpgradeSave.es3");

            // Save other properties
            ES3.Save("CurrentToolID", currentToolId, "UpgradeSave.es3");
            ES3.Save("MaxStamina", PlayerFuelManager.MaxFuelPoints, "UpgradeSave.es3");
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
            var upgrade = availableUpgrades.Find(u => u.upgradeTypeName == upgradeTypeName);

            // Safely handle when upgrade data is null
            if (upgrade == null)
            {
                UnityEngine.Debug.LogWarning($"No upgrade data found for {upgradeTypeName}");
                return 9999;
            }

            // Safely handle when level is out of bounds
            if (level < 0 || level >= upgrade.upgradeCosts.Length)
            {
                UnityEngine.Debug.LogWarning(
                    $"Level {level} is out of bounds for {upgradeTypeName} upgrades (max: {upgrade.upgradeCosts.Length - 1})");
                return 9999;
            }

            return upgrade.upgradeCosts[level];
        }

        public void LoadUpgrades()
        {
            UnityEngine.Debug.Log("Loading upgrades...");

            var isFreshStart = !ES3.KeyExists("ShovelMaterialLevel", "UpgradeSave.es3") &&
                               !ES3.KeyExists("Shovel", "UpgradeSave.es3");

            if (isFreshStart)
            {
                UnityEngine.Debug.Log("Fresh start detected - using initial materials");
                ApplyInitialMaterials();
            }

            // Load upgrade levels
            foreach (var upgrade in availableUpgrades)
                if (ES3.KeyExists(upgrade.upgradeTypeName, "UpgradeSave.es3"))
                {
                    UpgradeLevels[upgrade.upgradeTypeName] = ES3.Load<int>(upgrade.upgradeTypeName, "UpgradeSave.es3");
                    UnityEngine.Debug.Log(
                        $"Loaded upgrade level for {upgrade.upgradeTypeName}: {UpgradeLevels[upgrade.upgradeTypeName]}");
                }
                else
                {
                    UpgradeLevels[upgrade.upgradeTypeName] = 0;
                }

            // Load tool properties
            LoadToolProperties();

            // Apply material based on saved levels
            ApplyMaterialsBasedOnLevel();

            UnityEngine.Debug.Log("Finished loading all upgrades");
            isInitialLoad = false;
        }

        private void LoadToolProperties()
        {
            // Load shovel properties
            if (ES3.KeyExists("ShovelToolEffectRadius", "UpgradeSave.es3"))
                shovelToolEffectRadius = ES3.Load<float>("ShovelToolEffectRadius", "UpgradeSave.es3");

            if (ES3.KeyExists("ShovelToolEffectOpacity", "UpgradeSave.es3"))
                shovelToolEffectOpacity = ES3.Load<float>("ShovelToolEffectOpacity", "UpgradeSave.es3");

            if (ES3.KeyExists("ShovelToolWidth", "UpgradeSave.es3"))
                shovelToolWidth = ES3.Load<float>("ShovelToolWidth", "UpgradeSave.es3");

            if (ES3.KeyExists("ShovelMaterialLevel", "UpgradeSave.es3"))
            {
                shovelMaterialLevel = ES3.Load<int>("ShovelMaterialLevel", "UpgradeSave.es3");
                UnityEngine.Debug.Log($"Loaded shovel material level: {shovelMaterialLevel}");
            }

            // Apply shovel properties
            if (shovelTool != null)
                shovelTool.SetDiggerUsingToolEffectSize(shovelToolEffectRadius, shovelToolEffectOpacity);


            // Load pickaxe properties
            if (ES3.KeyExists("PickaxeToolEffectRadius", "UpgradeSave.es3"))
                pickaxeToolEffectRadius = ES3.Load<float>("PickaxeToolEffectRadius", "UpgradeSave.es3");

            if (ES3.KeyExists("PickaxeToolEffectOpacity", "UpgradeSave.es3"))
                pickaxeToolEffectOpacity = ES3.Load<float>("PickaxeToolEffectOpacity", "UpgradeSave.es3");

            if (ES3.KeyExists("PickaxeToolWidth", "UpgradeSave.es3"))
                pickaxeMiningToolWidth = ES3.Load<float>("PickaxeToolWidth", "UpgradeSave.es3");

            if (ES3.KeyExists("PickaxeMaterialLevel", "UpgradeSave.es3"))
            {
                pickaxeMaterialLevel = ES3.Load<int>("PickaxeMaterialLevel", "UpgradeSave.es3");
                UnityEngine.Debug.Log($"Loaded pickaxe material level: {pickaxeMaterialLevel}");
            }

            // Apply pickaxe properties
            if (pickaxeTool != null)
            {
                pickaxeTool.SetDiggerUsingToolEffectSize(pickaxeToolEffectRadius, pickaxeToolEffectOpacity);
                var oldScale = pickaxeTool.transform.localScale;
                pickaxeTool.transform.localScale = new Vector3(pickaxeMiningToolWidth, oldScale.y, oldScale.z);
            }

            // Load other properties
            if (ES3.KeyExists("MaxStamina", "UpgradeSave.es3"))
                PlayerFuelManager.MaxFuelPoints = ES3.Load<float>("MaxStamina", "UpgradeSave.es3");

            if (ES3.KeyExists("MaxFuelCapacity", "UpgradeSave.es3"))
                fuelCapacity = ES3.Load<float>("MaxFuelCapacity", "UpgradeSave.es3");

            if (ES3.KeyExists("InventoryMaxWeight", "GameSave.es3"))
            {
                var savedWeight = ES3.Load<float>("InventoryMaxWeight", "GameSave.es3");
                PlayerInventoryManager.SetWeightLimit(savedWeight);
            }

            if (ES3.KeyExists("CurrentToolID", "UpgradeSave.es3"))
                currentToolId = ES3.Load<string>("CurrentToolID", "UpgradeSave.es3");
        }

        private void ApplyInitialMaterials()
        {
            if (shovelTool != null)
            {
                shovelTool.SetCurrentMaterial(characterStatProfile.initialShovelMaterial);
                UnityEngine.Debug.Log("Applied initial GREY shovel material");
            }

            if (pickaxeTool != null)
            {
                pickaxeTool.SetCurrentMaterial(characterStatProfile.initialPickaxeMaterial);
                UnityEngine.Debug.Log("Applied initial pickaxe material");
            }
        }


        public string GetUpgradeName(string upgradeTypeName)
        {
            var level = GetUpgradeLevel(upgradeTypeName);
            var upgrade = availableUpgrades.Find(u => u.upgradeTypeName == upgradeTypeName);

            if (upgrade == null)
            {
                UnityEngine.Debug.LogWarning($"No upgrade data found for {upgradeTypeName}");
                return "Unknown Upgrade";
            }

            if (level < 0 || level >= upgrade.upgradeNames.Length)
            {
                UnityEngine.Debug.LogWarning(
                    $"Level {level} is out of bounds for {upgradeTypeName} upgrade names (max: {upgrade.upgradeNames.Length - 1})");
                return "Max Level Reached";
            }

            return upgrade.upgradeNames[level]; // Return the name for the current level
        }

        public bool HasSavedData()
        {
            return ES3.FileExists("UpgradeSave.es3");
        }

        private void ApplyMaterialsBasedOnLevel()
        {
            // Shovel material based on level
            if (shovelTool != null)
            {
                var shovelUpgrade = availableUpgrades.Find(u => u.upgradeTypeName == "Shovel");
                if (shovelUpgrade != null)
                {
                    var upgradedShovelLevel = GetUpgradeLevel("Shovel");

                    // Determine material based on level and material level
                    if (upgradedShovelLevel <= 0 && shovelMaterialLevel <= 0)
                    {
                        // Initial state - grey material
                        shovelTool.SetCurrentMaterial(characterStatProfile.initialShovelMaterial);
                        UnityEngine.Debug.Log("Applied initial GREY shovel material (no upgrades)");
                    }
                    else if (shovelMaterialLevel >= 0 && shovelMaterialLevel < shovelUpgrade.upgradeMaterials.Length)
                    {
                        // Upgraded state - use material from upgrade data
                        shovelTool.SetCurrentMaterial(shovelUpgrade.upgradeMaterials[shovelMaterialLevel]);
                        UnityEngine.Debug.Log($"Applied shovel material level {shovelMaterialLevel}");
                    }
                }
            }

            // Pickaxe material based on level
            if (pickaxeTool != null)
            {
                var pickaxeUpgrade = availableUpgrades.Find(u => u.upgradeTypeName == "Pickaxe");
                if (pickaxeUpgrade != null)
                {
                    var upgradedPickaxeLevel = GetUpgradeLevel("Pickaxe");

                    UnityEngine.Debug.Log(
                        $"Pickaxe upgrade level: {upgradedPickaxeLevel}, Material level: {pickaxeMaterialLevel}");

                    if (upgradedPickaxeLevel <= 0 && pickaxeMaterialLevel <= 0)
                    {
                        // Initial state
                        pickaxeTool.SetCurrentMaterial(characterStatProfile.initialPickaxeMaterial);
                        UnityEngine.Debug.Log("Applied initial GREY pickaxe material (no upgrades)");
                    }
                    else if (pickaxeMaterialLevel >= 0 && pickaxeMaterialLevel < pickaxeUpgrade.upgradeMaterials.Length)
                    {
                        // Upgraded state
                        var materialToApply = pickaxeUpgrade.upgradeMaterials[pickaxeMaterialLevel];
                        if (materialToApply != null)
                        {
                            pickaxeTool.SetCurrentMaterial(materialToApply);
                            UnityEngine.Debug.Log(
                                $"Applied pickaxe material level {pickaxeMaterialLevel}: {materialToApply.name}");
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"Pickaxe material at level {pickaxeMaterialLevel} is null!");
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("Could not find Pickaxe upgrade data!");
                }
            }
        }

        public static void ResetPlayerUpgrades()
        {
            UnityEngine.Debug.Log("ResetPlayerUpgrades called - resetting all upgrades to initial state");

            var characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);
            if (characterStatProfile == null)
            {
                UnityEngine.Debug.LogError(
                    "CharacterStatProfile not found in ResetPlayerUpgrades! Using default values.");
                return;
            }

            // Reset upgrade levels
            var upgradeKeys = new List<string>(UpgradeLevels.Keys);
            foreach (var key in upgradeKeys)
                UpgradeLevels[key] = characterStatProfile.InitialUpgradeState;

            // Reset tool properties
            shovelToolEffectRadius = characterStatProfile.initialShovelToolEffectRadius;
            shovelToolEffectOpacity = characterStatProfile.initialShovelToolEffectOpacity;
            shovelToolWidth = characterStatProfile.shovelMiningToolWidth;

            pickaxeToolEffectRadius = characterStatProfile.initialPickaxeToolEffectRadius;
            pickaxeToolEffectOpacity = characterStatProfile.pickaxeMiningToolEffectOpacity;
            pickaxeMiningToolWidth = characterStatProfile.pickaxeMiningToolWidth;

            // Reset material levels
            shovelMaterialLevel = 0;
            pickaxeMaterialLevel = 0;

            // Apply default materials
            if (shovelTool != null)
            {
                shovelTool.SetCurrentMaterial(characterStatProfile.initialShovelMaterial);
                shovelTool.SetDiggerUsingToolEffectSize(shovelToolEffectRadius, shovelToolEffectOpacity);
                UnityEngine.Debug.Log("Reset to initial GREY shovel material");
            }

            if (pickaxeTool != null)
            {
                pickaxeTool.SetCurrentMaterial(characterStatProfile.initialPickaxeMaterial);
                pickaxeTool.SetDiggerUsingToolEffectSize(pickaxeToolEffectRadius, pickaxeToolEffectOpacity);

                var oldScale = pickaxeTool.transform.localScale;
                pickaxeTool.transform.localScale = new Vector3(pickaxeMiningToolWidth, oldScale.y, oldScale.z);

                UnityEngine.Debug.Log("Reset to initial pickaxe material");
            }

            // Delete material-related keys to ensure clean state
            if (ES3.KeyExists("ShovelMaterialLevel", "UpgradeSave.es3"))
                ES3.DeleteKey("ShovelMaterialLevel", "UpgradeSave.es3");

            if (ES3.KeyExists("PickaxeMaterialLevel", "UpgradeSave.es3"))
                ES3.DeleteKey("PickaxeMaterialLevel", "UpgradeSave.es3");

            // Trigger events
            UpgradeEvent.Trigger(UpgradeType.Shovel, UpgradeEventType.ShovelMiningSizeSet, null, 0,
                UpgradeEffectType.None, shovelToolEffectRadius, null, shovelToolEffectOpacity);

            UpgradeEvent.Trigger(UpgradeType.Pickaxe, UpgradeEventType.PickaxeMiningSizeSet, null, 0,
                UpgradeEffectType.None, pickaxeToolEffectRadius, null, pickaxeToolEffectOpacity);
        }
//         public static void ResetPlayerUpgrades()
//         {
//             var characterStatProfile =
//                 Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);
//
//             if (characterStatProfile == null)
//             {
//                 UnityEngine.Debug.LogError("CharacterStatProfile not found! Using default values.");
//
//                 // 🔥 FIX: Store keys in a separate list before modifying dictionary
//                 var upgradeKeys = new List<string>(UpgradeLevels.Keys);
//                 foreach (var key in upgradeKeys) UpgradeLevels[key] = 0;
//             }
//             else
//             {
//                 // 🔥 FIX: Same approach, storing keys separately
//                 var upgradeKeys = new List<string>(UpgradeLevels.Keys);
//                 foreach (var key in upgradeKeys) UpgradeLevels[key] = characterStatProfile.InitialUpgradeState;
//             }
//
//             // Reset mining tool size to default value
//
//             shovelToolEffectRadius = characterStatProfile.initialShovelToolEffectRadius; // Use your default value here
//             pickaxeToolEffectRadius =
//                 characterStatProfile.initialPickaxeToolEffectRadius; // Use your default value here
//
//             // Tool width
//             shovelToolWidth = characterStatProfile.shovelMiningToolWidth; // Use your default value here
//             pickaxeMiningToolWidth =
//                 characterStatProfile.pickaxeMiningToolWidth; // Use your default value here
//
//             // Opacity
//
//             shovelToolEffectOpacity =
//                 characterStatProfile.initialShovelToolEffectOpacity; // Use your default value here
//             pickaxeToolEffectOpacity =
//                 characterStatProfile.pickaxeMiningToolEffectOpacity; // Use your default value here
//
//
//             var shovelUpgradeScriptableObject =
//                 characterStatProfile.initialShovelMaterial;
//             var pickaxeUpgradeScriptableObject =
//                 characterStatProfile.initialPickaxeMaterial;
//
//             shovelTool.SetCurrentMaterial(shovelUpgradeScriptableObject);
//             pickaxeTool.SetCurrentMaterial(pickaxeUpgradeScriptableObject);
//
//             // Find shovel upgrade data to GET the correct material
//             var shovelUpgradeData = Resources.FindObjectsOfTypeAll<UpgradeData>()
//                 .FirstOrDefault(u => u.upgradeTypeName == "Shovel");
//
// // Set appropriate material based on upgrade level
//             if (shovelUpgradeData != null)
//             {
//                 // If no upgrades purchased yet (initial state) or dictionary doesn't have the key
//                 if (!UpgradeLevels.ContainsKey("Shovel") || UpgradeLevels["Shovel"] <= 0)
//                 {
//                     // Use MaterialGrey (initial material)
//                     shovelTool.SetCurrentMaterial(characterStatProfile.initialShovelMaterial);
//                     UnityEngine.Debug.Log("Reset to initial grey material");
//                 }
//                 else if (UpgradeLevels["Shovel"] == 1)
//                 {
//                     // Use MaterialGreen (first upgrade level)
//                     shovelTool.SetCurrentMaterial(shovelUpgradeData.upgradeMaterials[0]);
//                     UnityEngine.Debug.Log("Reset to green material (level 1)");
//                 }
//                 else if (UpgradeLevels["Shovel"] == 2)
//                 {
//                     // Use MaterialBlue (second upgrade level)
//                     shovelTool.SetCurrentMaterial(shovelUpgradeData.upgradeMaterials[1]);
//                     UnityEngine.Debug.Log("Reset to blue material (level 2)");
//                 }
//             }
//
//
//             UpgradeEvent.Trigger(UpgradeType.Shovel, UpgradeEventType.ShovelMiningSizeSet, null, 0,
//                 UpgradeEffectType.None, shovelToolEffectRadius, null, shovelToolEffectOpacity);
//
//             UpgradeEvent.Trigger(UpgradeType.Pickaxe, UpgradeEventType.PickaxeMiningSizeSet, null, 0,
//                 UpgradeEffectType.None, pickaxeToolEffectRadius, null, pickaxeToolEffectOpacity);
//         }
    }
}