using System;
using System.Collections.Generic;
using Domains.Items.Events;
using Domains.Player.Events;
using Domains.UI.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Items
{
    public class Inventory : MonoBehaviour
    {
        public List<InventoryEntry> Content;

        public MMFeedbacks InventoryFullFeedbacks;

        private float _weightLimit;


        public float CurrentWeight()
        {
            float weight = 0;
            foreach (var item in Content) weight += item.BaseItem.ItemWeight;

            return weight;
        }

        public float RemainingWeight()
        {
            return Mathf.Max(0, _weightLimit - CurrentWeight()); // Prevent negative weight values
        }

        public virtual bool AddItem(InventoryEntry item)
        {
            if (CurrentWeight() + item.BaseItem.ItemWeight > _weightLimit)
            {
                UnityEngine.Debug.LogWarning("Inventory is full");
                InventoryFullFeedbacks?.PlayFeedbacks();
                AlertEvent.Trigger(AlertType.InventoryFull, "Inventory is full");
                return false;
            }

            Content.Add(item); // Always add as a new entry, even if identical items exist
            InventoryEvent.Trigger(InventoryEventType.ContentChanged, this);
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

        public virtual bool IsFull()
        {
            return CurrentWeight() >= _weightLimit;
        }

        public virtual void SaveInventory()
        {
            throw new NotImplementedException();
        }

        public void EmptyInventory()
        {
            Content = new List<InventoryEntry>();

            InventoryEvent.Trigger(InventoryEventType.ContentChanged, this);
        }

        public void SetWeightLimit(int weightLimit)
        {
            _weightLimit = weightLimit;
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