using Domains.Scripts_that_Need_Sorting;
using UnityEngine;

namespace Domains.Gameplay.Tools.ToolSpecifics
{
    public class ScannerTool : MonoBehaviour, IToolAction
    {
        public ScannerTool(ToolType toolType)
        {
            ToolType = toolType;
        }

        public ToolType ToolType { get; }

        public void UseTool(RaycastHit hit)
        {
        }
    }
}