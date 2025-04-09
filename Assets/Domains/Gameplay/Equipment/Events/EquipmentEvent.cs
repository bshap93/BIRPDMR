using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Tools;

namespace Domains.Gameplay.Equipment.Events
{
    public struct EquipmentEvent
    {
        private static EquipmentEvent _e;

        public ToolType ToolType;

        public static void Trigger(ToolType toolType
        )
        {
            _e.ToolType = toolType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}