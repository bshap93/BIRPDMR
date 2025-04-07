using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using Domains.Player.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Gameplay.Tools.ToolSpecifics
{
    public class ShovelTool : MonoBehaviour, IToolAction, MMEventListener<UpgradeEvent>
    {
        [Header("Dig Settings")] public float miningRange = 5f;

        [SerializeField] private float miningCooldown = 1f; // seconds between digs

        public float effectRadius = 1f;
        public float effectOpacity = 10f;
        public float stalagmiteHeight = 10f;
        public BrushType brush = BrushType.Stalagmite;
        public ActionType action = ActionType.Dig;
        public bool editAsynchronously = true;
        public Camera mainCamera;

        public MMFeedbacks diggingFeedbacks;


        [Header("FX")] public GameObject debrisEffectPrefab;

        [Header("Allowed Layers")] [Tooltip("Allowed Unity layers for GameObjects (e.g., ore nodes)")]
        public LayerMask interactableLayers;

        [Tooltip("Allowed texture indices on terrain")]
        public int[] allowedTerrainTextureIndices;

        private DiggerMasterRuntime digger;
        private float lastDigTime = -999f;
        private RaycastHit lastHit;
        private PlayerInteraction playerInteraction;

        private void Awake()
        {
            digger = FindFirstObjectByType<DiggerMasterRuntime>();
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public ToolType ToolType { get; }

        public void UseTool(RaycastHit hit)
        {
            lastHit = hit;
            PerformToolAction();
        }


        public void PerformToolAction()
        {
            if (Time.time < lastDigTime + miningCooldown)
                return;

            lastDigTime = Time.time;

            if (playerInteraction == null || digger == null)
                return;

            var notPlayerMask = ~playerInteraction.playerLayerMask;
            if (!Physics.Raycast(Camera.main.transform.position, mainCamera.transform.forward, out var hit, miningRange,
                    notPlayerMask))
                return;

            // Cache hit for external access
            lastHit = hit;

            // Interact
            hit.collider.GetComponent<IInteractable>()?.Interact();

            // Debris FX
            if (debrisEffectPrefab)
            {
                var pos = hit.point + hit.normal * 0.1f;
                var rot = Quaternion.LookRotation(-mainCamera.transform.forward);
                var fx = Instantiate(debrisEffectPrefab, pos, rot);
                Destroy(fx, 2f);
            }

            // Feedback trigger (from PerformToolAction, not MMFeedbacks directly)
            if (diggingFeedbacks != null) diggingFeedbacks.PlayFeedbacks(hit.point);

            // Dig!
            var digPosition = hit.point + mainCamera.transform.forward * 0.3f;
            var textureIndex = 1;

            if (editAsynchronously)
                digger.ModifyAsyncBuffured(digPosition, brush, action, textureIndex, effectOpacity, effectRadius,
                    stalagmiteHeight);
            else
                digger.Modify(digPosition, brush, action, textureIndex, effectOpacity, effectRadius);

            FuelEvent.Trigger(FuelEventType.ConsumeFuel, 2f, PlayerFuelManager.MaxFuelPoints);
        }

        public bool CanInteractWithTextureIndex(int index)
        {
            foreach (var allowed in allowedTerrainTextureIndices)
                if (index == allowed)
                    return true;
            return false;
        }

        public bool CanInteractWithObject(GameObject target)
        {
            return (interactableLayers.value & (1 << target.layer)) != 0;
        }

        public void OnMMEvent(UpgradeEvent eventType)
        {
            if (eventType.EventType == UpgradeEventType.ShovelMiningSizeSet)
                SetShovelEffectSize(eventType.EffectValue, eventType.EffectValue2);
        }

        public void SetShovelEffectSize(float newEffectRadius, float newEffectOpacity)
        {
            // Apply safety limits
            var minSize = 0.4f;
            var maxSize = 1.2f;

            // Validate and apply size
            effectRadius = Mathf.Clamp(newEffectRadius, minSize, maxSize);
            effectOpacity = Mathf.Clamp(newEffectOpacity, 5f, 25f);

            // Log the assigned size for debugging
            UnityEngine.Debug.Log($"ShovelMiningState.size set to: {effectRadius}, {effectOpacity}");
        }
    }
}