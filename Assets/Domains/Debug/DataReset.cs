using Domains.Player.Events;
using Domains.Player.Scripts;
using Domains.Scene.Scripts;
using UnityEngine;

namespace Domains.Debug
{
    public class DataReset : MonoBehaviour
    {
        private void Awake()
        {
            UnityEngine.Debug.Log("PurePrototypeReset: Awake() called.");
            ClearAllSaveData();
        }

        public static void ClearAllSaveData()
        {
            var isEditorMode = !Application.isPlaying;

            if (isEditorMode) UnityEngine.Debug.Log("Running data reset in Editor mode...");

            // Reset stats
            PlayerStaminaManager.ResetPlayerStamina();
            PlayerStaminaManager.SavePlayerStamina();

            // Reset health
            PlayerHealthManager.ResetPlayerHealth();
            PlayerHealthManager.SavePlayerHealth();

            // Reset inventory
            if (PlayerInventoryManager.Instance != null)
            {
                PlayerInventoryManager.ResetInventory();

                PlayerInventoryManager.Instance.SaveInventory();
            }
            else
            {
                UnityEngine.Debug.LogError("PlayerInventoryManager.Instance is null. Skipping inventory reset.");
            }

            // Reset currency
            PlayerCurrencyManager.ResetPlayerCurrency();
            PlayerCurrencyManager.SavePlayerCurrency();

            // reset pickables
            PickableManager.ResetPickedItems();
            PickableManager.SaveAllPickedItems();

            DestructableManager.ResetDestructables();
            DestructableManager.SaveAllDestructables();

            // Reset upgrades
            PlayerUpgradeManager.ResetPlayerUpgrades();
            PlayerUpgradeManager.SaveUpgrades();

            // Try reset Digger. Note this won't work in editor mode
            DiggerEvent.Trigger(DiggerEventType.Delete);

            // Reset digger data if it exists
            // if (DiggerDataManager.Instance != null) DiggerDataManager.Instance.ResetDiggerData();


            UnityEngine.Debug.Log("All save data cleared successfully.");
        }
    }
}