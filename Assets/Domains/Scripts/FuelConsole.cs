using Domains.Player.Events;
using Domains.Player.Scripts;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Scripts
{
    public class FuelConsole : MonoBehaviour, MMEventListener<UIEvent>
    {
        public int fuelPricePerUnit = 10;
        private MMFeedbacks buyFuelFeedbacks;
        private FuelUIController fuelUIController;

        private int playerCurrencyAmount;
        private float playerFuelRemaining;

        private void Start()
        {
            playerFuelRemaining = PlayerFuelManager.FuelPoints;
            playerCurrencyAmount = PlayerCurrencyManager.CompanyCredits;
            fuelUIController = FindFirstObjectByType<FuelUIController>();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(UIEvent eventType)
        {
            if (eventType.EventType == UIEventType.UpdateFuelConsole) UpdateFuelUI();
        }

        private void UpdateFuelUI()
        {
            var playerCredits = PlayerCurrencyManager.CompanyCredits;
            var maxFuel = PlayerFuelManager.MaxFuelPoints;
            var currentFuel = PlayerFuelManager.FuelPoints;
            var fuelPlayerCanAfford = playerCredits / fuelPricePerUnit;
            var fuelToBuy = Mathf.Min(fuelPlayerCanAfford, maxFuel - currentFuel);
            var costOfFuelToBuy = Mathf.FloorToInt(fuelToBuy * fuelPricePerUnit);

            fuelUIController.UpdateFuelUI(currentFuel, maxFuel, fuelPricePerUnit, playerCredits, fuelToBuy,
                costOfFuelToBuy);
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
            var maxFuel = PlayerFuelManager.MaxFuelPoints;
            var currentFuel = PlayerFuelManager.FuelPoints;
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
            var maxFuel = PlayerFuelManager.MaxFuelPoints;
            var currentFuel = PlayerFuelManager.FuelPoints;
            var fuelPlayerCanAfford = playerCredits / fuelPricePerUnit;
            var fuelToBuy = Mathf.Min(fuelPlayerCanAfford, maxFuel - currentFuel);
            var costOfFuelToBuy = Mathf.FloorToInt(fuelToBuy * fuelPricePerUnit);

            if (costOfFuelToBuy > 0)
            {
                CurrencyEvent.Trigger(CurrencyEventType.RemoveCurrency, costOfFuelToBuy);
                FuelEvent.Trigger(FuelEventType.RecoverFuel,
                    fuelToBuy, PlayerFuelManager.MaxFuelPoints);

                buyFuelFeedbacks?.PlayFeedbacks();
            }
        }
    }
}