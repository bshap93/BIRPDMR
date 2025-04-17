using Domains.Items.Events;
using Domains.Items.Inventory;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.UI_Global.Triggers
{
    public class SellAllItemsButtonTrigger : MonoBehaviour
    {
        [FormerlySerializedAs("SellAllFeedbacks")]
        public MMFeedbacks sellAllFeedbacks;

        public MMFeedbacks cannotSellAllFeedbacks;

        private Inventory _inventory;

        private void Start()
        {
            _inventory = FindFirstObjectByType<Inventory>();
        }


        public void TriggerSellAll()
        {
            if (_inventory.IsEmpty)
            {
                CannotSellAll();
                return;
            }

            sellAllFeedbacks?.PlayFeedbacks();
            InventoryEvent.Trigger(InventoryEventType.SellAllItems, _inventory, 0);
        }

        public void CannotSellAll()
        {
            AlertEvent.Trigger(AlertReason.InventotryEmpty, "You cannot sell all items when the inventory is empty.",
                "Inventory is empty");
            cannotSellAllFeedbacks?.PlayFeedbacks();
        }
    }
}