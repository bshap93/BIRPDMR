using Domains.Gameplay.Equipment.Scripts;
using Domains.Gameplay.Mining.Events;
using UnityEngine;

namespace Domains.Scripts_that_Need_Sorting
{
    public class MiningBehavior : MonoBehaviour
    {
        [SerializeField] private ToolIteration toolIteration;
        [SerializeField] private ToolType toolType;

        private void Start()
        {
            GetTool();
        }

        private void GetTool()
        {
            toolIteration = PlayerEquipment.Instance.currentToolIteration;
            toolType = PlayerEquipment.Instance.currentToolType;
        }

        public void OnMining()
        {
            GetTool();
            switch (toolType)
            {
                case ToolType.MiningTool:
                    switch (toolIteration)
                    {
                        case ToolIteration.First:
                            ToolEvent.Trigger(ToolEventType.UseTool, ToolType.MiningTool, ToolIteration.First);
                            break;
                        case ToolIteration.Second:
                            ToolEvent.Trigger(ToolEventType.UseTool, ToolType.MiningTool, ToolIteration.Second);
                            break;
                    }

                    break;
            }
        }
    }
}