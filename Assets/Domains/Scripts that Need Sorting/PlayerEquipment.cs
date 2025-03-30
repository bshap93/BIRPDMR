using Domains.Input.Scripts;
using UnityEngine;

namespace Domains.Scripts_that_Need_Sorting
{
    public class PlayerEquipment : MonoBehaviour
    {
        public static PlayerEquipment Instance;

        public GameObject miningTool;
        public GameObject scanningTool;

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
            SwitchTool(0);
        }

        private void Update()
        {
            if (CustomInputBindings.IsChangingWeapons())
            {
                int direction = CustomInputBindings.GetWeaponChangeDirection();
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
                currentToolType = ToolType.Scanner;
                currentToolIteration = ToolIteration.First;
            }
            else if (index == 1)
            {
                miningTool.SetActive(false);
                scanningTool.SetActive(true);
                currentToolType = ToolType.Scanner;
                currentToolIteration = ToolIteration.First;
            }
        }
    }
}