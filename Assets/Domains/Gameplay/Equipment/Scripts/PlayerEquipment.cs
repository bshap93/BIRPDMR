using Domains.Gameplay.Equipment.Events;
using Domains.Gameplay.Tools;
using Domains.Gameplay.Tools.ToolSpecifics;
using Domains.Input.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Equipment.Scripts
{
    public class PlayerEquipment : MonoBehaviour
    {
        public static PlayerEquipment Instance;

        public ShovelTool shovelTool;
        public ScannerTool scannerTool;

        [SerializeField] private MMFeedbacks equipMinerFeedbacks;
        [SerializeField] private MMFeedbacks equipScannerFeedbacks;

        public int numTools;

        public ToolType currentToolType;
        public ToolIteration currentToolIteration;

        [SerializeField] private int currentToolIndex;
        public IToolAction CurrentToolComponent;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CurrentToolComponent = shovelTool;
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
            if (CurrentToolComponent == null)
                UnityEngine.Debug.LogWarning($"Tool at index {index} is missing an IToolAction component.");
            currentToolIndex = index;

            if (index == 0)
            {
                shovelTool.gameObject.SetActive(true);
                scannerTool.gameObject.SetActive(false);
                currentToolType = ToolType.Shovel;
                currentToolIteration = ToolIteration.First;
                EquipmentEvent.Trigger(EquipmentEventType.EquipShovel);
                equipMinerFeedbacks?.PlayFeedbacks();

                // Set the current tool component
                CurrentToolComponent = shovelTool;
            }
            else if (index == 1)
            {
                shovelTool.gameObject.SetActive(false);
                scannerTool.gameObject.SetActive(true);
                currentToolType = ToolType.Scanner;
                currentToolIteration = ToolIteration.First;
                EquipmentEvent.Trigger(EquipmentEventType.EquipScanner);
                equipScannerFeedbacks?.PlayFeedbacks();

                // Set the current tool component
                CurrentToolComponent = scannerTool;
            }
            else if (index == 2)
            {
                shovelTool.gameObject.SetActive(false);
                scannerTool.gameObject.SetActive(false);
                currentToolType = ToolType.Pickaxe;
                currentToolIteration = ToolIteration.First;
                EquipmentEvent.Trigger(EquipmentEventType.EquipPickaxe);
                CurrentToolComponent = null;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Invalid tool index: {index}");
            }
        }
    }
}