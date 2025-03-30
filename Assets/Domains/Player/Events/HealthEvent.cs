using System;
using MoreMountains.Tools;

namespace Domains.Player.Events
{
    [Serializable]
    public enum HealthEventType
    {
        ConsumeHealth,
        RecoverHealth,
        FullyRecoverHealth,
        IncreaseMaximumHealth,
        DecreaseMaximumHealth,
        SetCurrentHealth
    }

    public struct HealthEvent
    {
        private static HealthEvent e;

        public HealthEventType EventType;
        public float ByValue;

        public static void Trigger(HealthEventType healthEventType,
            float byValue)
        {
            e.EventType = healthEventType;
            e.ByValue = byValue;
            MMEventManager.TriggerEvent(e);
        }
    }
}