using System;
using MoreMountains.Tools;

namespace Domains.UI.Events
{
    [Serializable]
    public enum AlertType
    {
        ItemScrapped,
        InventoryFull
    }

    public struct AlertEvent
    {
        public static AlertEvent _e;

        public AlertType AlertType;
        public string AlertMessage;

        public static void Trigger(AlertType alertType, string alertMessage)
        {
            _e.AlertType = alertType;
            _e.AlertMessage = alertMessage;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
