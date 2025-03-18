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

            // Reset Pickables - only if not in editor mode, or make PickableManager.ResetPickedItems() editor-safe
            if (!isEditorMode)
            {
                PickableManager.ResetPickedItems();
            }
            else
            {
                // Direct file manipulation for Editor mode
                if (ES3.FileExists(SaveManager.SaveFileName))
                    if (ES3.KeyExists("PickedItems", SaveManager.SaveFileName))
                    {
                        ES3.DeleteKey("PickedItems", SaveManager.SaveFileName);
                        UnityEngine.Debug.Log("Reset picked items data");
                    }
            }
            

            // Reset stats
            PlayerStaminaManager.ResetPlayerStamina();
            PlayerHealthManager.ResetPlayerHealth();
            PlayerInventoryManager.ResetInventory();
            PlayerCurrencyManager.ResetPlayerCurrency();
            PickableManager.ResetPickedItems();
            PlayerUpgradeManager.ResetPlayerUpgrades();


            UnityEngine.Debug.Log("All save data cleared successfully.");
        }
    }
}