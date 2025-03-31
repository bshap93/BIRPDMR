using Domains.Player.Events;
using Domains.Scene.Scripts;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Player.Scripts
{
    public class PlayerDeathManager : MonoBehaviour, MMEventListener<PlayerStatusEvent>

    {
        private static PlayerDeathManager _instance;

        public int monetaryPenalty = 400;

        [FormerlySerializedAs("staminaPenaltyMultiplier")]
        public float fuelPenaltyMultiplier = 0.2f;

        public float healthPenaltyMultiplier = 0.2f;

        public MMFeedbacks deathFeedbacks;
        public MMFeedbacks outOfFuelFeedbacks;

        [FormerlySerializedAs("_sceneRestarter")] [SerializeField]
        private MMSceneRestarter sceneRestarter;

        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject playerCamera;


        private void Awake()
        {
            _instance = this;
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
            }

            if (eventType.EventType == PlayerStatusEventType.OutOfFuel)
            {
                SetPostFuelOutStats();
                outOfFuelFeedbacks?.PlayFeedbacks();
                AlertEvent.Trigger(AlertReason.OutOfFuel, "You are out of fuel!", "Out of Fuel");
            }
        }

        public void SetPostFuelOutStats()
        {
            var maximumFuel = PlayerFuelManager.MaxFuelPoints;


            FuelEvent.Trigger(FuelEventType.SetCurrentFuel, fuelPenaltyMultiplier * maximumFuel, maximumFuel);
            CurrencyEvent.Trigger(CurrencyEventType.LoseCurrency, monetaryPenalty);

            SaveManager.Instance.SaveAll();
        }


        public void SetPostDeathStats()
        {
            var maximumHealth = PlayerHealthManager.MaxHealthPoints;
            var maxFuel = PlayerFuelManager.MaxFuelPoints;
            var currentCurrency = PlayerCurrencyManager.CompanyCredits;
            FuelEvent.Trigger(FuelEventType.SetCurrentFuel, fuelPenaltyMultiplier * maxFuel, maxFuel);
            HealthEvent.Trigger(HealthEventType.SetCurrentHealth, healthPenaltyMultiplier * maximumHealth);
            CurrencyEvent.Trigger(CurrencyEventType.LoseCurrency, monetaryPenalty);

            SaveManager.Instance.SaveAll();
        }
    }
}