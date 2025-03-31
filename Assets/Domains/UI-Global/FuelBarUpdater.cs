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

        [FormerlySerializedAs("textPlaceholderCurrentStamina")] [FormerlySerializedAs("textPlaceholder")]
        public TMP_Text textPlaceholderCurrentFuel;

        [FormerlySerializedAs("textPlaceholderMaxStamina")]
        public TMP_Text textPlaceholderMaxFuel;

        private MMProgressBar _bar;
        private float _currentFuel;

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
            _currentFuel = PlayerFuelManager.FuelPoints;
            _maxFuel = PlayerFuelManager.MaxFuelPoints;

            UpdateUI();
        }

        private void UpdateUI()
        {
            // Update text elements if using text mode
            if (useTextPlaceholder)
            {
                textPlaceholderCurrentFuel.text = _currentFuel.ToString();
                textPlaceholderMaxFuel.text = _maxFuel.ToString();
            }

            // Always update the progress bar if it exists
            if (_bar != null) _bar.UpdateBar(_currentFuel, 0, _maxFuel);
        }


        public void Initialize()
        {
            _maxFuel = PlayerFuelManager.MaxFuelPoints;
            _currentFuel = PlayerFuelManager.FuelPoints;
            if (useTextPlaceholder)
            {
                textPlaceholderCurrentFuel.text = _currentFuel.ToString();
                textPlaceholderMaxFuel.text = _maxFuel.ToString();
            }
            else
            {
                _bar.UpdateBar(_currentFuel, 0, _maxFuel);
            }
        }
    }
}