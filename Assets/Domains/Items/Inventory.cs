using System;
using System.Collections.Generic;
using Domains.Items.Events;
using Domains.Player.Events;
using Domains.Scene.Scripts;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Items
{
    public class Inventory : MonoBehaviour
    {
        public List<InventoryEntry> Content;

        public MMFeedbacks InventoryFullFeedbacks;


        public virtual bool AddItem(InventoryEntry item)
        {
            if (PlayerInventoryManager.GetCurrentWeight() + item.BaseItem.ItemWeight >
                PlayerInventoryManager.GetWeightLimit())
            {
                UnityEngine.Debug.LogWarning("Inventory is full");
                InventoryFullFeedbacks?.PlayFeedbacks();
                AlertEvent.Trigger(AlertType.InventoryFull, "Inventory is full");
                return false;
            }

            Content.Add(item); // Always add as a new entry, even if identical items exist
            InventoryEvent.Trigger(InventoryEventType.ContentChanged, this, 0);
            return true;
        }


        public virtual bool RemoveItem(InventoryEntry item)
        {
            if (!Content.Contains(item))
            {
                UnityEngine.Debug.LogWarning("Item not found in inventory");
                return false;
            }

            return Content.Remove(item);
        }

        public virtual bool RemoveItem(string uniqueID)
        {
            var item = Content.Find(i => i.UniqueID == uniqueID);
            if (item == null)
            {
                UnityEngine.Debug.LogWarning("Item not found in inventory");
                return false;
            }

            return Content.Remove(item);
        }

        public virtual int GetQuantity(string itemID)
        {
            return Content.FindAll(i => i.BaseItem.ItemID == itemID).Count;
        }

        public virtual InventoryEntry GetItem(string uniqueID)
        {
            return Content.Find(i => i.UniqueID == uniqueID) ?? null;
        }

        public virtual List<int> InventoryContainsItemType(string searchedItemID)
        {
            var list = new List<int>();

            for (var i = 0; i < Content.Count; i++)
                if (Content[i].BaseItem.ItemID == searchedItemID)
                    list.Add(i);

            return list;
        }

        // public virtual bool IsFull()
        // {
        //     return CurrentWeight() >= _weightLimit;
        // }

        public virtual void SaveInventory()
        {
            throw new NotImplementedException();
        }

        public void EmptyInventory()
        {
            Content = new List<InventoryEntry>();

            InventoryEvent.Trigger(InventoryEventType.ContentChanged, this, 0);
        }


        public void SellAllItems()
        {
            var totalValue = 0;
            foreach (var item in Content)
            {
                var value = item.BaseItem.ItemValue;

                totalValue += value;
            }

            CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, totalValue);
            EmptyInventory();
        }

        [Serializable]
        public class InventoryEntry
        {
            public string UniqueID;
            public BaseItem BaseItem;

            public InventoryEntry(string uniqueID, BaseItem item)
            {
                UniqueID = uniqueID;
                BaseItem = item;
            }
        }
    }
}