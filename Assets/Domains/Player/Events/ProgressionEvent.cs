using MoreMountains.Tools;

namespace Domains.Player.Events
{
    public enum ProgressionEventType
    {
    }

    public struct ProgressionEvent
    {
        public static ProgressionEvent _e;

        public ProgressionEventType EventType;

        public static void Trigger(ProgressionEventType eventType)
        {
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}