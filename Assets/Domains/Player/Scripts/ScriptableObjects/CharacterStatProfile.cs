using UnityEngine;

namespace Domains.Player.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CharacterStatProfile", menuName = "Character/Character Stat Profile")]
    public class CharacterStatProfile : ScriptableObject
    {
        [Header("Initial Stats")] public float InitialMaxStamina;
        public float InitialMaxHealth;


        [Header("Initial Endurance")] public int InitialEnduranceLevel;
        public int InitialEnduranceExperiencePoints;

        [Header("Inventory Stats")] public int InitialWeightLimit;
        [Header("Currency Stats")] public int InitialCurrency;

        [Header("Upgrades")] public int InitialUpgradeState;
    }
}