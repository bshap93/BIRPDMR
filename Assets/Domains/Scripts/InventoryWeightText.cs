using System.Globalization;
using Domains.Items.Events;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class InventoryWeightText : MonoBehaviour, MMEventListener<InventoryEvent>
{
    TMP_Text _text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
    }


    void OnEnable()
    {
        this.MMEventStartListening();
    }

    void OnDisable()
    {
        this.MMEventStopListening();
    }
    public void OnMMEvent(InventoryEvent eventType)
    {
        if (eventType.EventType == InventoryEventType.ContentChanged)
            _text.text = eventType.Inventory.CurrentWeight().ToString(CultureInfo.InvariantCulture);
    }
}
