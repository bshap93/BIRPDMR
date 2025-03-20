using System;
using JetBrains.Annotations;
using MoreMountains.Tools;
using UnityEngine;

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
        [CanBeNull] public string AlertTitle;
        [CanBeNull] public Sprite AlertIcon;

        public static void Trigger(AlertType alertType, string alertMessage, string alertTitle = "Alert",
            Sprite alertIcon = null)
        {
            _e.AlertType = alertType;
            _e.AlertMessage = alertMessage;
            _e.AlertTitle = alertTitle;
            _e.AlertIcon = alertIcon;
            MMEventManager.TriggerEvent(_e);
        }
    }
}