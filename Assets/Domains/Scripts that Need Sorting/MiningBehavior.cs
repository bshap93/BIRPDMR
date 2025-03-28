using Domains.Gameplay.Mining.Events;
using Domains.Scripts_that_Need_Sorting;
using UnityEngine;

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
            case ToolType.Shovel:
                switch (toolIteration)
                {
                    case ToolIteration.Smallest:
                        ToolEvent.Trigger(ToolEventType.UseTool, ToolType.Shovel, ToolIteration.Smallest);
                        break;
                    case ToolIteration.SecondSmallest:
                        ToolEvent.Trigger(ToolEventType.UseTool, ToolType.Shovel, ToolIteration.SecondSmallest);
                        break;
                }

                break;
        }
    }
}