using MoreMountains.Tools;

namespace Domains.UI_Global.Events
{
    public enum UIEventType
    {
        OpenVendorConsole,
        CloseVendorConsole
    }

    public struct UIEvent
    {
        private static UIEvent _e;

        public UIEventType EventType;

        public static void Trigger(UIEventType eventType)
        {
            _e.EventType = eventType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}