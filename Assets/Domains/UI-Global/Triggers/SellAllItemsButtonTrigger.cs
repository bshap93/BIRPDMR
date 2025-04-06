using Domains.Items.Events;
using Domains.Items.Inventory;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.UI_Global.Triggers
{
    public class SellAllItemsButtonTrigger : MonoBehaviour
    {
        [FormerlySerializedAs("SellAllFeedbacks")]
        public MMFeedbacks sellAllFeedbacks;

        private Inventory _inventory;

        public void TriggerSellAll()
        {
            sellAllFeedbacks?.PlayFeedbacks();
            InventoryEvent.Trigger(InventoryEventType.SellAllItems, _inventory, 0);
        }
    }
}