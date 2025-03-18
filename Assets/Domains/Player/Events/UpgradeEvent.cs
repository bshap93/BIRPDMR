using Domains.Player.Scripts.ScriptableObjects;
using MoreMountains.Tools;

namespace Domains.Player.Events
{
    public enum UpgradeEventType
    {
        UpgradePurchased,
        UpgradeFailed
    }

    public struct UpgradeEvent
    {
        private static UpgradeEvent _e;

        public UpgradeEventType EventType;

        public UpgradeData UpgradeData;
        public int UpgradeLevel;


        public static void Trigger(UpgradeEventType upgradeEventType,
            UpgradeData upgradeData, int upgradeLevel)
        {
            _e.EventType = upgradeEventType;
            _e.UpgradeData = upgradeData;
            _e.UpgradeLevel = upgradeLevel;

            MMEventManager.TriggerEvent(_e);
        }
    }
}