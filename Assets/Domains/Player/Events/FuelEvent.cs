using System;
using MoreMountains.Tools;

namespace Domains.Player.Events
{
    [Serializable]
    public enum FuelEventType
    {
        ConsumeStamina,
        RecoverStamina,
        FullyRecoverStamina,
        IncreaseMaximumStamina,
        DecreaseMaximumStamina,
        Initialize,
        SetMaxStamina,
        SetCurrentStamina
    }

    public struct FuelEvent
    {
        private static FuelEvent _e;

        public FuelEventType EventType;
        public float ByValue;

        public static void Trigger(FuelEventType fuelEventType,
            float byValue)
        {
            _e.EventType = fuelEventType;
            _e.ByValue = byValue;
            MMEventManager.TriggerEvent(_e);
        }
    }
}