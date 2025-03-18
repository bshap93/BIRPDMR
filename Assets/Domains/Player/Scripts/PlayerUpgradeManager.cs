using System.Collections.Generic;
using Domains.Player.Events;
using Domains.Player.Scripts.ScriptableObjects;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public class PlayerUpgradeManager : MonoBehaviour, MMEventListener<UpgradeEvent>
    {
        private static readonly Dictionary<string, int> UpgradeLevels = new();
        private static Dictionary<string, string> _upgradeNames = new();
        [SerializeField] private List<UpgradeData> availableUpgrades;

        public MMFeedbacks upgradeFeedback;

        private void Awake()
        {
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

            if (PlayerCurrencyManager.CompanyCredits >= cost)
            {
                CurrencyEvent.Trigger(CurrencyEventType.RemoveCurrency, cost);
                UpgradeLevels[upgradeTypeName]++;
                SaveUpgrades();

                // 🔥 Update UI with the new upgrade name
                upgradeFeedback?.PlayFeedbacks();
                UpgradeEvent.Trigger(UpgradeEventType.UpgradePurchased, upgrade, UpgradeLevels[upgradeTypeName]);

                UpdateUI();
            }
            else
            {
                UpgradeEvent.Trigger(UpgradeEventType.UpgradeFailed, upgrade, UpgradeLevels[upgradeTypeName]);
                UnityEngine.Debug.Log("Not enough credits!");
            }
        }


        private void UpdateUI()
        {
            // Update UI
        }

        public static void SaveUpgrades()
        {
            foreach (var upgrade in UpgradeLevels) ES3.Save(upgrade.Key, upgrade.Value, "UpgradeSave.es3");
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

            SaveUpgrades();
        }
    }
}