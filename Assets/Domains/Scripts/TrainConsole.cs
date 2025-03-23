using Domains.Items;
using Domains.Items.Events;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

public class TrainConsole : MonoBehaviour
{
    [FormerlySerializedAs("SellAllFeedbacks")]
    public MMFeedbacks sellAllFeedbacks;

    private Inventory _inventory;


    private void Start()
    {
        _inventory = FindFirstObjectByType<Inventory>();
    }


    public void TriggerSellAll()
    {
        sellAllFeedbacks?.PlayFeedbacks();
        InventoryEvent.Trigger(InventoryEventType.SellAllItems, _inventory, 0);
    }

    public void TriggerOpenVendorUI()
    {
        UIEvent.Trigger(UIEventType.OpenVendorConsole);
    }
}