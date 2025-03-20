using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay.Character.Stamina
{
    public class StaminaBarUpdater : MonoBehaviour, MMEventListener<StaminaEvent>

    {
        public bool useTextPlaceholder = true;

        [FormerlySerializedAs("textPlaceholder")]
        public TMP_Text textPlaceholderCurrentStamina;

        public TMP_Text textPlaceholderMaxStamina;
        private MMProgressBar _bar;
        private float _currentStamina;

        private float _maxStamina;

        private void Awake()
        {
            if (useTextPlaceholder)
            {
            }
            else
            {
                _bar = GetComponent<MMProgressBar>();
            }
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(StaminaEvent eventType)
        {
            if (useTextPlaceholder)
                switch (eventType.EventType)
                {
                    case StaminaEventType.ConsumeStamina:
                        _currentStamina -= eventType.ByValue;
                        textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                        break;
                    case StaminaEventType.RecoverStamina:
                        _currentStamina += eventType.ByValue;
                        textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                        break;
                    case StaminaEventType.FullyRecoverStamina:
                        _currentStamina = _maxStamina;
                        textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                        break;
                    case StaminaEventType.IncreaseMaximumStamina:
                        _maxStamina += eventType.ByValue;
                        textPlaceholderMaxStamina.text = _maxStamina.ToString();
                        break;
                    case StaminaEventType.DecreaseMaximumStamina:
                        _maxStamina -= eventType.ByValue;
                        textPlaceholderMaxStamina.text = _maxStamina.ToString();
                        break;
                    case StaminaEventType.SetMaxStamina:
                        _maxStamina = eventType.ByValue;
                        textPlaceholderMaxStamina.text = _maxStamina.ToString();
                        break;
                }
            else
                switch (eventType.EventType)
                {
                    case StaminaEventType.ConsumeStamina:
                        _currentStamina -= eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxStamina);
                        break;
                    case StaminaEventType.RecoverStamina:
                        _currentStamina += eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxStamina);
                        break;
                    case StaminaEventType.FullyRecoverStamina:
                        _currentStamina = _maxStamina;
                        _bar.UpdateBar(_currentStamina, 0, _maxStamina);
                        break;
                    case StaminaEventType.IncreaseMaximumStamina:
                        _maxStamina += eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxStamina);
                        break;
                    case StaminaEventType.SetMaxStamina:
                        _maxStamina = eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxStamina);
                        break;
                }
        }

        public void Initialize()
        {
            _maxStamina = PlayerStaminaManager.MaxStaminaPoints;
            _currentStamina = PlayerStaminaManager.StaminaPoints;
            if (useTextPlaceholder)
            {
                textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                textPlaceholderMaxStamina.text = _maxStamina.ToString();
            }
            else
            {
                _bar.UpdateBar(_currentStamina, 0, _maxStamina);
            }
        }
    }
}