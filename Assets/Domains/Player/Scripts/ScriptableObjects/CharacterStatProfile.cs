using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Player.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CharacterStatProfile", menuName = "Character/Character Stat Profile")]
    public class CharacterStatProfile : ScriptableObject
    {
        [FormerlySerializedAs("InitialMaxStamina")] [Header("Initial Stats")]
        public float InitialMaxFuel;

        public float InitialMaxHealth;

        public Color initialUpgradeColor;

        [Header("Material Settings")] public Material initialPickaxeMaterial;

        public Material initialShovelMaterial;
        public Material initialScannerMaterial;


        [Header("Inventory Stats")] public int InitialWeightLimit;
        [Header("Currency Stats")] public int InitialCurrency;

        [Header("Upgrades")] public int InitialUpgradeState;

        [FormerlySerializedAs("InitialMiningToolSize")] [Header("Tool Stats")]
        public float initialShovelToolEffectRadius;

        public float initialPickaxeToolEffectRadius;

        [Header("Skip Tutorial")] public bool SkipTutorial;

        [FormerlySerializedAs("MiningToolWidth")] [Header("Mining Tool Width")]
        public float shovelMiningToolWidth;

        public float pickaxeMiningToolWidth;

        [FormerlySerializedAs("MiningToolOpacity")] [Header("Mining Tool Height")]
        public float initialShovelToolEffectOpacity;

        public float pickaxeMiningToolEffectOpacity;
    }
}