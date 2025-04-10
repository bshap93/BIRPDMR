using Domains.Gameplay.Equipment.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Gameplay.Tools.ToolSpecifics
{
    public class ScannerTool : MonoBehaviour, IToolAction
    {
        [SerializeField] private ToolType toolType;
        [SerializeField] private ToolIteration toolIteration;
        [SerializeField] private MMFeedbacks equipFeedbacks;
        [FormerlySerializedAs("textureDetector")] [SerializeField] private TerrainLayerDetector terrainLayerDetector;
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private float maxToolRange = 5f;
        [SerializeField] private Camera mainCamera;
        private RaycastHit lastHit;

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

        public int GetCurrentTextureIndex()
        {
            var notPlayerMask = ~playerMask;
            if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit,
                    maxToolRange, notPlayerMask))
            {
                var tool = PlayerEquipment.Instance.CurrentToolComponent;
                if (tool == null) return -1;

                // Get terrain texture index at hit point
                Terrain terrain;
                var textureIndex = terrainLayerDetector.GetTextureIndex(hit, out terrain);


                return textureIndex;
            }

            return -1;
        }
    }
}