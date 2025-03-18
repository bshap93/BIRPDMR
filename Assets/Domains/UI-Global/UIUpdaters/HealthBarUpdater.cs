using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace Domains.UI
{
    public class HealthBarUpdater : MonoBehaviour, MMEventListener<HealthEvent>
    {
        public bool useTextPlaceholder = true;
        public TMP_Text textPlaceholderCurrentHealth;
        public TMP_Text textPlaceholderMaxHealth;
        MMProgressBar _bar;
        float _currentHealth;

        float _maxHealth;

        void Awake()
        {
            if (useTextPlaceholder)
            {
            }
            else
            {
                _bar = GetComponent<MMProgressBar>();
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(HealthEvent eventType)
        {
            if (useTextPlaceholder)
                switch (eventType.EventType)
                {
                    case HealthEventType.ConsumeHealth:
                        _currentHealth -= eventType.ByValue;
                        textPlaceholderCurrentHealth.text = _currentHealth.ToString();
                        break;
                    case HealthEventType.RecoverHealth:
                        _currentHealth += eventType.ByValue;
                        textPlaceholderCurrentHealth.text = _currentHealth.ToString();
                        break;
                    case HealthEventType.FullyRecoverHealth:
                        _currentHealth = _maxHealth;
                        textPlaceholderCurrentHealth.text = _currentHealth.ToString();
                        break;
                    case HealthEventType.IncreaseMaximumHealth:
                        _maxHealth += eventType.ByValue;
                        textPlaceholderMaxHealth.text = _maxHealth.ToString();
                        break;
                    case HealthEventType.DecreaseMaximumHealth:
                        _maxHealth -= eventType.ByValue;
                        textPlaceholderMaxHealth.text = _maxHealth.ToString();
                        break;
                }
            else
                switch (eventType.EventType)
                {
                    case HealthEventType.ConsumeHealth:
                        _currentHealth -= eventType.ByValue;
                        _bar.UpdateBar(_currentHealth, 0, _maxHealth);
                        break;
                    case HealthEventType.RecoverHealth:
                        _currentHealth += eventType.ByValue;
                        _bar.UpdateBar(_currentHealth, 0, _maxHealth);
                        break;
                    case HealthEventType.FullyRecoverHealth:
                        _currentHealth = _maxHealth;
                        _bar.UpdateBar(_currentHealth, 0, _maxHealth);
                        break;
                    case HealthEventType.IncreaseMaximumHealth:
                        _maxHealth += eventType.ByValue;
                        _bar.UpdateBar(_currentHealth, 0, _maxHealth);
                        break;
                    case HealthEventType.DecreaseMaximumHealth:
                        _maxHealth -= eventType.ByValue;
                        _bar.UpdateBar(_currentHealth, 0, _maxHealth);
                        break;
                }
        }


        public void Initialize()
        {
            _maxHealth = PlayerHealthManager.MaxHealthPoints;
            _currentHealth = PlayerHealthManager.HealthPoints;
            if (useTextPlaceholder)
            {
                textPlaceholderCurrentHealth.text = _currentHealth.ToString();
                textPlaceholderMaxHealth.text = _maxHealth.ToString();
            }
            else
            {
                _bar.UpdateBar(_currentHealth, 0, _maxHealth);
            }
        }
    }
}
