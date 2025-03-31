using Domains.Player.Events;
using Domains.Scene.Scripts;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public class PlayerDeathManager : MonoBehaviour, MMEventListener<PlayerStatusEvent>

    {
        private static PlayerDeathManager _instance;

        public int monetaryPenalty = 400;
        public float staminaPenaltyMultiplier = 0.2f;
        public float healthPenaltyMultiplier = 0.2f;

        public MMFeedbacks deathFeedbacks;
        public MMFeedbacks outOfFuelFeedbacks;

        private MMSceneRestarter _sceneRestarter;

        private void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            _sceneRestarter = GetComponent<MMSceneRestarter>();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(PlayerStatusEvent eventType)
        {
            if (eventType.EventType == PlayerStatusEventType.Died)
            {
                SetPostDeathStats();
                deathFeedbacks?.PlayFeedbacks();
                AlertEvent.Trigger(AlertReason.Died, "You have died!", "Game Over");
                _sceneRestarter.RestartScene();
            }

            if (eventType.EventType == PlayerStatusEventType.OutOfFuel)
            {
                SetPostFuelOutStats();
                outOfFuelFeedbacks?.PlayFeedbacks();
                AlertEvent.Trigger(AlertReason.OutOfFuel, "You are out of fuel!", "Out of Fuel");
                _sceneRestarter.RestartScene();
            }
        }

        public void SetPostFuelOutStats()
        {
            var maximumFuel = PlayerFuelManager.MaxFuelPoints;

            FuelEvent.Trigger(FuelEventType.SetCurrentStamina, staminaPenaltyMultiplier * maximumFuel);
            CurrencyEvent.Trigger(CurrencyEventType.LoseCurrency, monetaryPenalty);

            SaveManager.Instance.SaveAll();
        }

        public void SetPostDeathStats()
        {
            var maximumHealth = PlayerHealthManager.MaxHealthPoints;
            var maximumStamina = PlayerFuelManager.MaxFuelPoints;
            var currentCurrency = PlayerCurrencyManager.CompanyCredits;
            FuelEvent.Trigger(FuelEventType.SetCurrentStamina, staminaPenaltyMultiplier * maximumStamina);
            HealthEvent.Trigger(HealthEventType.SetCurrentHealth, healthPenaltyMultiplier * maximumHealth);
            CurrencyEvent.Trigger(CurrencyEventType.LoseCurrency, monetaryPenalty);

            SaveManager.Instance.SaveAll();
        }
    }
}