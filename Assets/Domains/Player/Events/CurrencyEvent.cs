using MoreMountains.Tools;

namespace Domains.Player.Events
{
    public enum CurrencyEventType
    {
        AddCurrency,
        RemoveCurrency,
        SetCurrency
    }

    public struct CurrencyEvent
    {
        public CurrencyEventType EventType;
        public int Amount;

        public static void Trigger(CurrencyEventType eventType, int amount)
        {
            var currencyEvent = new CurrencyEvent
            {
                EventType = eventType,
                Amount = amount
            };

            MMEventManager.TriggerEvent(currencyEvent);
        }
    }
}