using Domains.Items;
using Domains.Items.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

public class SellAllItemsButtonTrigger : MonoBehaviour
{
    private Inventory _inventory;

    [FormerlySerializedAs("SellAllFeedbacks")]
    public MMFeedbacks sellAllFeedbacks;

    public void TriggerSellAll()
    {
        sellAllFeedbacks?.PlayFeedbacks();
        InventoryEvent.Trigger(InventoryEventType.SellAllItems, _inventory);
    }
}