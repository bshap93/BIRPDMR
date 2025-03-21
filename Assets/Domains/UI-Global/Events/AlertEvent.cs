using System;
using JetBrains.Annotations;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.UI_Global.Events
{
    [Serializable]
    public enum AlertType
    {
        ItemScrapped,
        InventoryFull,
        Test,
        InsufficientFunds,
        OutOfStamina,
        HealthHitZero,
        SavingGame,
        DeletingDiggerData
    }

    public struct AlertEvent
    {
        public static AlertEvent _e;

        public AlertType AlertType;
        public string AlertMessage;
        [CanBeNull] public string AlertTitle;
        [CanBeNull] public Sprite AlertIcon;
        [CanBeNull] public AudioClip AlertSound;
        [CanBeNull] public Color AlertColor;

        public static void Trigger(AlertType alertType, string alertMessage, string alertTitle = "Alert",
            Sprite alertIcon = null,
            AudioClip alertSound = null, Color alertColor = default)
        {
            _e.AlertType = alertType;
            _e.AlertMessage = alertMessage;
            _e.AlertTitle = alertTitle;
            _e.AlertIcon = alertIcon;
            MMEventManager.TriggerEvent(_e);
        }
    }
}