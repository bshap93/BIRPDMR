using MoreMountains.Tools;

namespace Domains.Gameplay.Equipment.Events
{
    public enum EquipmentEventType
    {
        EquipScanner,
        EquipShovel,
        EquipPickaxe
    }

    public struct EquipmentEvent
    {
        private static EquipmentEvent _e;

        public EquipmentEventType EventType;

        public static void Trigger(EquipmentEventType equipmentEventType
        )
        {
            _e.EventType = equipmentEventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}