using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.UI_Global
{
    public class FuelBarUpdater : MonoBehaviour, MMEventListener<FuelEvent>

    {
        public bool useTextPlaceholder = true;

        [FormerlySerializedAs("textPlaceholder")]
        public TMP_Text textPlaceholderCurrentStamina;

        public TMP_Text textPlaceholderMaxStamina;
        private MMProgressBar _bar;
        private float _currentStamina;

        private float _maxFuel;

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

        public void OnMMEvent(FuelEvent eventType)
        {
            if (useTextPlaceholder)
                switch (eventType.EventType)
                {
                    case FuelEventType.ConsumeStamina:
                        _currentStamina -= eventType.ByValue;
                        textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                        break;
                    case FuelEventType.RecoverStamina:
                        _currentStamina += eventType.ByValue;
                        textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                        break;
                    case FuelEventType.FullyRecoverStamina:
                        _currentStamina = _maxFuel;
                        textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                        break;
                    case FuelEventType.IncreaseMaximumStamina:
                        _maxFuel += eventType.ByValue;
                        textPlaceholderMaxStamina.text = _maxFuel.ToString();
                        break;
                    case FuelEventType.DecreaseMaximumStamina:
                        _maxFuel -= eventType.ByValue;
                        textPlaceholderMaxStamina.text = _maxFuel.ToString();
                        break;
                    case FuelEventType.SetMaxStamina:
                        _maxFuel = eventType.ByValue;
                        textPlaceholderMaxStamina.text = _maxFuel.ToString();
                        UnityEngine.Debug.Log($"Updated Max Stamina to {_maxFuel}"); // Debugging
                        if (_bar != null)
                            _bar.UpdateBar(_currentStamina, 0, _maxFuel);
                        break;
                }
            else
                switch (eventType.EventType)
                {
                    case FuelEventType.ConsumeStamina:
                        _currentStamina -= eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxFuel);
                        break;
                    case FuelEventType.RecoverStamina:
                        _currentStamina += eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxFuel);
                        break;
                    case FuelEventType.FullyRecoverStamina:
                        _currentStamina = _maxFuel;
                        _bar.UpdateBar(_currentStamina, 0, _maxFuel);
                        break;
                    case FuelEventType.IncreaseMaximumStamina:
                        _maxFuel += eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxFuel);
                        break;
                    case FuelEventType.SetMaxStamina:
                        _maxFuel = eventType.ByValue;
                        _bar.UpdateBar(_currentStamina, 0, _maxFuel);
                        break;
                }
        }

        public void Initialize()
        {
            _maxFuel = PlayerFuelManager.MaxFuelPoints;
            _currentStamina = PlayerFuelManager.FuelPoints;
            if (useTextPlaceholder)
            {
                textPlaceholderCurrentStamina.text = _currentStamina.ToString();
                textPlaceholderMaxStamina.text = _maxFuel.ToString();
            }
            else
            {
                _bar.UpdateBar(_currentStamina, 0, _maxFuel);
            }
        }
    }
}