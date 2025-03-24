using Domains.Player.Scripts;
using MoreMountains.Tools;

namespace Domains.Player.Events
{
    public enum PointedObjectEventType
    {
        PointedObjectChanged
    }

    public struct PointedObjectEvent
    {
        private static PointedObjectEvent e;

        public PointedObjectEventType eventType;
        public string name;


        public static void Trigger(PointedObjectEventType pointedObjectEventType,
            string objName)
        {
            e.eventType = pointedObjectEventType;
            e.name = objName;
            MMEventManager.TriggerEvent(e);
        }
    }
}