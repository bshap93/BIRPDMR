using System;
using MoreMountains.Tools;

namespace Domains.Player.Events
{
    [Serializable]
    public enum StaminaEventType
    {
        ConsumeStamina,
        RecoverStamina,
        FullyRecoverStamina,
        IncreaseMaximumStamina,
        DecreaseMaximumStamina,
        Initialize
    }

    public struct StaminaEvent
    {
        static StaminaEvent _e;

        public StaminaEventType EventType;
        public float ByValue;

        public static void Trigger(StaminaEventType staminaEventType,
            float byValue)
        {
            _e.EventType = staminaEventType;
            _e.ByValue = byValue;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
