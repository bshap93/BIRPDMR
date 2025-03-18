using System;
using MoreMountains.Tools;

namespace Domains.Player.Scripts
{
    [Serializable]
    public enum PlayerStatusEventType
    {
        OutOfStamina,
        OutOfHealth,
        RegainedHealth,
        ImmuneToDamage,
        ResetHealth,
        ResetStamina,
        RegainedStamina
    }

    public struct PlayerStatusEvent
    {
        private static PlayerStatusEvent e;

        public PlayerStatusEventType EventType;

        public static void Trigger(PlayerStatusEventType eventType)
        {
            e.EventType = eventType;
            MMEventManager.TriggerEvent(e);
        }
    }
}