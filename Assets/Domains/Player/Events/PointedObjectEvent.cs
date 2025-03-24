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
        public PointedObjectInfo pointedObjectInfo;


        public static void Trigger(PointedObjectEventType pointedObjectEventType, PointedObjectInfo pointedObjectInfo)
        {
            UnityEngine.Debug.Log("Name: " + pointedObjectInfo.name);
            e.eventType = pointedObjectEventType;
            e.pointedObjectInfo = pointedObjectInfo;
            MMEventManager.TriggerEvent(e);
        }
    }
}