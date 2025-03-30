using Domains.Player.Scripts;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Scripts
{
    public class FuelConsole : MonoBehaviour
    {
        public float fuelPricePerUnit = 10f;
        private MMFeedbacks buyFuelFeedbacks;

        private int playerCurrencyAmount;
        private float playerFuelRemaining;

        private void Start()
        {
            playerFuelRemaining = PlayerStaminaManager.StaminaPoints;
            playerCurrencyAmount = PlayerCurrencyManager.CompanyCredits;
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public void TriggerOpenFuelUI()
        {
            UIEvent.Trigger(UIEventType.OpenFuelConsole);
        }
    }
}