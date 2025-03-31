using System;
using JetBrains.Annotations;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.UI_Global.Events
{
    [Serializable]
    public enum AlertReason
    {
        InventoryFull,
        Test,
        InsufficientFunds,
        OutOfFuel,
        SavingGame,
        DeletingDiggerData,
        Died
    }

    public struct AlertEvent
    {
        public static AlertEvent _e;

        public AlertReason AlertReason;
        public string AlertMessage;
        [CanBeNull] public string AlertTitle;
        [CanBeNull] public Sprite AlertIcon;
        [CanBeNull] public AudioClip AlertSound;
        [CanBeNull] public Color AlertColor;

        public static void Trigger(AlertReason alertReason, string alertMessage, string alertTitle = "Alert",
            Sprite alertIcon = null,
            AudioClip alertSound = null, Color alertColor = default)
        {
            _e.AlertReason = alertReason;
            _e.AlertMessage = alertMessage;
            _e.AlertTitle = alertTitle;
            _e.AlertIcon = alertIcon;
            MMEventManager.TriggerEvent(_e);
        }
    }
}