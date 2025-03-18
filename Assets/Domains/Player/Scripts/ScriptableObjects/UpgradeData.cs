using UnityEngine;

namespace Domains.Player.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        public string upgradeTypeName;
        public string[] upgradeNames;
        public string description;
        public int[] upgradeCosts;
    }
}