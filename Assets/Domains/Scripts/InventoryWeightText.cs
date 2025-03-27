using System.Globalization;
using Domains.Items.Events;
using Domains.Scene.Scripts;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace Domains.Scripts
{
    public class InventoryWeightText : MonoBehaviour, MMEventListener<InventoryEvent>
    {
        private TMP_Text _text;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            _text = GetComponent<TMP_Text>();
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
                var currentWeight = PlayerInventoryManager.Instance.GetCurrentWeight();
                var maxWeight = PlayerInventoryManager.Instance.GetMaxWeight();
                if (_text == null || _text.text == null) return;
                _text.text =
                    $"{currentWeight.ToString(CultureInfo.InvariantCulture)} / {maxWeight.ToString(CultureInfo.InvariantCulture)}";
                // _text.text = eventType.Inventory.CurrentWeight().ToString(CultureInfo.InvariantCulture);
            }
            else if (eventType.EventType == InventoryEventType.UpgradedWeightLimit)
            {
                var maxWeight = PlayerInventoryManager.Instance.GetMaxWeight();
                _text.text =
                    $"{PlayerInventoryManager.Instance.GetCurrentWeight().ToString(CultureInfo.InvariantCulture)} / {maxWeight.ToString(CultureInfo.InvariantCulture)}";
            }
        }
    }
}