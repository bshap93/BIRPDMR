using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Tools.ToolSpecifics
{
    public class ScannerTool : MonoBehaviour, IToolAction
    {
        [SerializeField] private ToolType toolType;
        [SerializeField] private ToolIteration toolIteration;
        [SerializeField] private MMFeedbacks equipFeedbacks;

        public ToolType ToolType => toolType;
        public ToolIteration ToolIteration => toolIteration;
        public MMFeedbacks EquipFeedbacks => equipFeedbacks;


        public void UseTool(RaycastHit hit)
        {
        }

        public void PerformToolAction()
        {
        }

        public bool CanInteractWithTextureIndex(int index)
        {
            return false;
        }


        public bool CanInteractWithObject(GameObject target)
        {
            return false;
        }
    }
}