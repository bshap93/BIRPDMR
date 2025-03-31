using Domains.Gameplay.Equipment.Events;
using Domains.Input.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Equipment.Scripts
{
    public class PlayerEquipment : MonoBehaviour
    {
        public static PlayerEquipment Instance;

        public GameObject miningTool;
        public GameObject scanningTool;

        [SerializeField] private MMFeedbacks equipMinerFeedbacks;
        [SerializeField] private MMFeedbacks equipScannerFeedbacks;

        public int numTools;

        public ToolType currentToolType;
        public ToolIteration currentToolIteration;

        private int currentToolIndex;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (CustomInputBindings.IsChangingWeapons())
            {
                var direction = CustomInputBindings.GetWeaponChangeDirection();
                currentToolIndex = (currentToolIndex + direction + numTools) % numTools;
                SwitchTool(currentToolIndex);
            }
        }

        private void SwitchTool(int index)
        {
            currentToolIndex = index;

            if (index == 0)
            {
                miningTool.SetActive(true);
                scanningTool.SetActive(false);
                currentToolType = ToolType.MiningTool;
                currentToolIteration = ToolIteration.First;
                EquipmentEvent.Trigger(EquipmentEventType.SwitchFromScanner);
                EquipmentEvent.Trigger(EquipmentEventType.EquipMiner);
                equipMinerFeedbacks?.PlayFeedbacks();
            }
            else if (index == 1)
            {
                miningTool.SetActive(false);
                scanningTool.SetActive(true);
                currentToolType = ToolType.Scanner;
                currentToolIteration = ToolIteration.First;
                EquipmentEvent.Trigger(EquipmentEventType.SwitchFromMiner);
                EquipmentEvent.Trigger(EquipmentEventType.EquipScanner);
                equipScannerFeedbacks?.PlayFeedbacks();
            }
        }
    }
}