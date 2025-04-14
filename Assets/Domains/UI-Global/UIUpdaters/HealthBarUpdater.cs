using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Domains.UI_Global.UIUpdaters
{
    public class HealthBarUpdater : MonoBehaviour, MMEventListener<HealthEvent>
    {
        [SerializeField] private Slider healthBarSlider;
        [SerializeField] private TMP_Text healthPercentageText;
        private float _currentHealth;

        private float _maxHealth;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(HealthEvent eventType)
        {
            switch (eventType.EventType)
            {
                case HealthEventType.ConsumeHealth:
                    _currentHealth -= eventType.ByValue;

                    break;
                case HealthEventType.RecoverHealth:
                    _currentHealth += eventType.ByValue;
                    // 
                    break;
                case HealthEventType.FullyRecoverHealth:
                    _currentHealth = _maxHealth;
                    //
                    break;
                case HealthEventType.IncreaseMaximumHealth:
                    _maxHealth += eventType.ByValue;
                    //  
                    break;
                case HealthEventType.DecreaseMaximumHealth:
                    _maxHealth -= eventType.ByValue;
                    // 
                    break;
            }

            UpdateBar();
        }


        public void Initialize()
        {
            _maxHealth = PlayerHealthManager.MaxHealthPoints;
            _currentHealth = PlayerHealthManager.HealthPoints;
            UpdateBar();
        }

        private void UpdateBar()
        {
            healthBarSlider.value = _currentHealth / _maxHealth;
            healthPercentageText.text = $"{healthBarSlider.value * 100:0}%";
        }
    }
}