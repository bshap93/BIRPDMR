using System.Globalization;
using Domains.Items.Events;
using Domains.Scene.Scripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace Domains.Items.Inventory
{
    public class InventoryWeightText : MonoBehaviour, MMEventListener<InventoryEvent>
    {
        private TMP_Text _text;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _text = GetComponent<TMP_Text>();
            UpdateWeightText();
        }


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(InventoryEvent eventType)
        {
            if (eventType.EventType == InventoryEventType.ContentChanged)
            {
                UpdateWeightText();
            }
            else if (eventType.EventType == InventoryEventType.UpgradedWeightLimit)
            {
                var maxWeight = PlayerInventoryManager.GetMaxWeight();
                _text.text =
                    $"{PlayerInventoryManager.GetCurrentWeight().ToString(CultureInfo.InvariantCulture)} / {maxWeight.ToString(CultureInfo.InvariantCulture)}";
            }
        }

        private void UpdateWeightText()
        {
            var currentWeight = PlayerInventoryManager.GetCurrentWeight();
            var maxWeight = PlayerInventoryManager.GetMaxWeight();
            if (_text == null || _text.text == null) return;
            _text.text =
                $"{currentWeight.ToString(CultureInfo.InvariantCulture)} / {maxWeight.ToString(CultureInfo.InvariantCulture)}";
            // _text.text = eventType.Inventory.CurrentWeight().ToString(CultureInfo.InvariantCulture);
        }
    }
}