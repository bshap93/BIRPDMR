using System;
using System.Collections.Generic;
using System.Linq;
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

        // New method to get grouped items
        public List<GroupedItem> GetGroupedItems()
        {
            var groupedItems = new List<GroupedItem>();
            var itemGroups = Content
                .GroupBy(item => item.BaseItem.ItemID)
                .Select(group => new
                {
                    ItemID = group.Key,
                    Items = group.ToList()
                });

            foreach (var group in itemGroups)
                if (group.Items.Count > 0)
                {
                    // Use the first item's BaseItem as the template
                    var baseItem = group.Items[0].BaseItem;
                    // Collect all unique IDs for this group
                    var uniqueIDs = group.Items.Select(item => item.UniqueID).ToList();

                    groupedItems.Add(new GroupedItem(
                        baseItem,
                        group.Items.Count,
                        uniqueIDs
                    ));
                }

            return groupedItems;
        }


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

        // Add this new class to represent grouped items
        [Serializable]
        public class GroupedItem
        {
            public BaseItem Item;
            public int Quantity;
            public List<string> UniqueIDs;

            public GroupedItem(BaseItem item, int quantity, List<string> uniqueIDs)
            {
                Item = item;
                Quantity = quantity;
                UniqueIDs = uniqueIDs;
            }
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