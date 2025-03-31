using Domains.Player.Events;
using Domains.Player.Scripts;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Scripts
{
    public class FuelConsole : MonoBehaviour
    {
        public int fuelPricePerUnit = 10;
        private MMFeedbacks buyFuelFeedbacks;
        private FuelUIController fuelUIController;

        private int playerCurrencyAmount;
        private float playerFuelRemaining;

        private void Start()
        {
            playerFuelRemaining = PlayerStaminaManager.StaminaPoints;
            playerCurrencyAmount = PlayerCurrencyManager.CompanyCredits;
            fuelUIController = FindFirstObjectByType<FuelUIController>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public void TriggerOpenFuelUI()
        {
            if (fuelUIController == null)
            {
                UnityEngine.Debug.LogError("FuelUIController not found in the scene.");
                return;
            }

            var playerCredits = PlayerCurrencyManager.CompanyCredits;
            var maxFuel = PlayerStaminaManager.MaxStaminaPoints;
            var currentFuel = PlayerStaminaManager.StaminaPoints;
            var fuelPlayerCanAfford = playerCredits / fuelPricePerUnit;
            var fuelToBuy = Mathf.Min(fuelPlayerCanAfford, maxFuel - currentFuel);
            var costOfFuelToBuy = Mathf.FloorToInt(fuelToBuy * fuelPricePerUnit);

            fuelUIController.UpdateFuelUI(currentFuel, maxFuel, fuelPricePerUnit, playerCredits, fuelToBuy,
                costOfFuelToBuy);

            UIEvent.Trigger(UIEventType.OpenFuelConsole);
        }

        public void BuyFuelPlayerCanAfford()
        {
            var playerCredits = PlayerCurrencyManager.CompanyCredits;
            var maxFuel = PlayerStaminaManager.MaxStaminaPoints;
            var currentFuel = PlayerStaminaManager.StaminaPoints;
            var fuelPlayerCanAfford = playerCredits / fuelPricePerUnit;
            var fuelToBuy = Mathf.Min(fuelPlayerCanAfford, maxFuel - currentFuel);
            var costOfFuelToBuy = Mathf.FloorToInt(fuelToBuy * fuelPricePerUnit);

            if (costOfFuelToBuy > 0)
            {
                CurrencyEvent.Trigger(CurrencyEventType.RemoveCurrency, costOfFuelToBuy);
                StaminaEvent.Trigger(StaminaEventType.RecoverStamina, fuelToBuy);

                buyFuelFeedbacks?.PlayFeedbacks();
            }
        }
    }
}