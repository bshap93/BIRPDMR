using Domains.Gameplay.Equipment.Events;
using Domains.Gameplay.Tools;
using Domains.Input.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Equipment.Scripts
{
    public class PlayerEquipment : MonoBehaviour
    {
        public static PlayerEquipment Instance;


        [SerializeField] private MMFeedbacks equipMinerFeedbacks;
        [SerializeField] private MMFeedbacks equipScannerFeedbacks;

        public ToolType currentToolType;
        public ToolIteration currentToolIteration;

        [SerializeField] private int currentToolIndex;

        [SerializeField] private MonoBehaviour[] toolBehaviours; // Shown in Inspector

        public IToolAction CurrentToolComponent;

        private int numTools;

        public IToolAction[] Tools { get; private set; } // Used in code


        private void Awake()
        {
            Instance = this;

            // Convert MonoBehaviours to IToolAction
            Tools = new IToolAction[toolBehaviours.Length];
            for (var i = 0; i < toolBehaviours.Length; i++)
            {
                Tools[i] = toolBehaviours[i] as IToolAction;
                if (Tools[i] == null) UnityEngine.Debug.LogError($"Tool at index {i} does not implement IToolAction.");
            }
        }

        private void Start()
        {
            numTools = Tools.Length;
            CurrentToolComponent = Tools[0];
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

            var tool = Tools[index];

            if (tool == null)
            {
                UnityEngine.Debug.LogWarning($"Tool at index {index} is null.");
                return;
            }

            currentToolType = tool.ToolType;
            currentToolIteration = tool.ToolIteration;
            CurrentToolComponent = tool;

            // Disable all tools
            foreach (var t in Tools)
                if (t is MonoBehaviour monoBehaviour)
                    monoBehaviour.gameObject.SetActive(false);

            // Enable the selected tool
            if (tool is MonoBehaviour mbh) mbh.gameObject.SetActive(true);

            // Trigger the appropriate events and feedbacks
            EquipmentEvent.Trigger(currentToolType);
            tool.EquipFeedbacks?.PlayFeedbacks();


            // if (index == 0)
            // {
            //     shovelTool.gameObject.SetActive(true);
            //     scannerTool.gameObject.SetActive(false);
            //     currentToolType = ToolType.Shovel;
            //     currentToolIteration = ToolIteration.First;
            //     EquipmentEvent.Trigger(EquipmentEventType.EquipShovel);
            //     equipMinerFeedbacks?.PlayFeedbacks();
            //
            //     // Set the current tool component
            //     CurrentToolComponent = shovelTool;
            // }
            // else if (index == 1)
            // {
            //     shovelTool.gameObject.SetActive(false);
            //     scannerTool.gameObject.SetActive(true);
            //     currentToolType = ToolType.Scanner;
            //     currentToolIteration = ToolIteration.First;
            //     EquipmentEvent.Trigger(EquipmentEventType.EquipScanner);
            //     equipScannerFeedbacks?.PlayFeedbacks();
            //
            //     // Set the current tool component
            //     CurrentToolComponent = scannerTool;
            // }
            // else if (index == 2)
            // {
            //     shovelTool.gameObject.SetActive(false);
            //     scannerTool.gameObject.SetActive(false);
            //     currentToolType = ToolType.Pickaxe;
            //     currentToolIteration = ToolIteration.First;
            //     EquipmentEvent.Trigger(EquipmentEventType.EquipPickaxe);
            //     CurrentToolComponent = null;
            // }
            // else
            // {
            //     UnityEngine.Debug.LogWarning($"Invalid tool index: {index}");
            // }
        }
    }
}