using MoreMountains.Tools;

namespace Domains.Items.Events
{
    public enum InventoryEventType
    {
        ContentChanged,
        InventoryLoaded,
        SellAllItems
    }

    public struct InventoryEvent
    {
        public static InventoryEvent E;

        public InventoryEventType EventType;

        public Inventory Inventory;

        public static void Trigger(InventoryEventType eventType, Inventory inventory)
        {
            E.EventType = eventType;
            E.Inventory = inventory;

            MMEventManager.TriggerEvent(E);
        }
    }
}