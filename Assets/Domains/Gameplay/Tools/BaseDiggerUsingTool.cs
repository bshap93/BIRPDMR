using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Player.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Gameplay.Tools
{
    public abstract class BaseDiggerUsingTool : MonoBehaviour, IToolAction
    {
        [Header("Dig Settings")] public float diggerUsingRange = 5f;

        [Header("Effect Settings")] public float minEffectRadius = 0.4f;

        public float maxEffectRadius = 1.2f;
        public float minEffectOpacity = 5f;
        public float maxEffectOpacity = 150f;
        [SerializeField] protected float miningCooldown = 1f; // seconds between digs

        [Header("Tool Settings")]
        [Tooltip("Feedbacks to play when the tool is used")]
        [FormerlySerializedAs("moveShovelDespiteFailHitFeedbacks")]
        [SerializeField]
        protected MMFeedbacks moveToolDespiteFailHitFeedbacks;

        [SerializeField] protected TerrainMineable terrainMineable;

        public float effectRadius = 1f;
        public float effectOpacity = 10f;
        public float stalagmiteHeight = 100f;

        public BrushType brush = BrushType.Stalagmite;
        public ActionType action = ActionType.Dig;
        public bool editAsynchronously = true;
        public Camera mainCamera;

        [Header("Feedbacks")] [Tooltip("Feedbacks to play when the tool cannot interact with an object")]
        public MMFeedbacks cannotInteractFeedbacks;


        [Header("FX")] public GameObject debrisEffectPrefab;

        [Header("Allowed Layers")] [Tooltip("Allowed Unity layers for GameObjects (e.g., ore nodes)")]
        public LayerMask diggableLayers;

        [Tooltip("Allowed texture indices on terrain")]
        public int[] allowedTerrainTextureIndices;

        [SerializeField] protected ToolType toolType;
        [SerializeField] protected ToolIteration toolIteration;
        [SerializeField] protected MMFeedbacks equipFeedbacks;

        [FormerlySerializedAs("textureDetector")] [FormerlySerializedAs("_textureDetector")] [SerializeField]
        protected TerrainLayerDetector terrainLayerDetector;

        private float currentDepth;

        protected DiggerMasterRuntime digger;
        protected float lastDigTime = -999f;
        protected RaycastHit lastHit;
        protected PlayerInteraction playerInteraction;

        public ToolType ToolType => toolType;
        public ToolIteration ToolIteration => toolIteration;
        public MMFeedbacks EquipFeedbacks => equipFeedbacks;

        public abstract void UseTool(RaycastHit hit);

        public abstract void PerformToolAction();

        public bool CanInteractWithTextureIndex(int index)
        {
            foreach (var allowed in allowedTerrainTextureIndices)
                if (index == allowed)
                    return true;
            return false;
        }

        public bool CanInteractWithObject(GameObject target)
        {
            return (diggableLayers.value & (1 << target.layer)) != 0;
        }

        public int GetCurrentTextureIndex()
        {
            var forwardTextureIndex = terrainLayerDetector.GetTextureIndex(lastHit, out _);


            return forwardTextureIndex;
        }


        public void SetDiggerUsingToolEffectSize(float newEffectRadius, float newEffectOpacity)
        {
            // Apply safety limits


            // Validate and apply size
            effectRadius = Mathf.Clamp(newEffectRadius, minEffectRadius, maxEffectRadius);
            effectOpacity = Mathf.Clamp(newEffectOpacity, minEffectOpacity, maxEffectOpacity);

            // Log the assigned size for debugging
            UnityEngine.Debug.Log($"ShovelMiningState.size set to: {effectRadius}, {effectOpacity}");
        }
    }
}