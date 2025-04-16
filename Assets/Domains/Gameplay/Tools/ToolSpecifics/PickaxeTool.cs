using System.Collections;
using System.Linq;
using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Gameplay.Tools.ToolSpecifics
{
    public class PickaxeTool : BaseDiggerUsingTool, MMEventListener<UpgradeEvent>
    {
        [Header("Stat Settings")] public int hardnessCanBreak;

        public DecalCrackSpawner crackSpawner;
        [SerializeField] private float delayBeforeDigging = 0.1f;

        [SerializeField] private MMFeedbacks firstHitFeedbacks;
        [SerializeField] private MMFeedbacks secondHitFeedbacks;
        [Header("Debris Effects")] public GameObject debrisEffectFirstHitPrefab;

        [SerializeField] private float firstHitEffectOpacity;
        [SerializeField] private float firstHitEffectRadius;

        public GameObject debrisEffectSecondHitPrefab;

        [FormerlySerializedAs("diggingFeedbacks")] [SerializeField]
        private MMFeedbacks pickaxeBehavior;

        public Material currentMaterial;

        [Header("Hit Number Logic")] [SerializeField]
        private readonly float hitThresholdDistance = 0.5f; // adjust as needed


        private Vector3 lastHitPosition;
        private MeshRenderer meshRenderer;
        private int terrainHitCount;


        private void Awake()
        {
            digger = FindFirstObjectByType<DiggerMasterRuntime>();
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
            // Reapply current material when enabled
            if (currentMaterial != null)
            {
                SetCurrentMaterial(currentMaterial);
                UnityEngine.Debug.Log($"{GetType().Name} enabled - reapplied material: {currentMaterial.name}");
            }
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(UpgradeEvent eventType)
        {
            if (eventType.EventType == UpgradeEventType.PickaxeMiningSizeSet)
                SetDiggerUsingToolEffectSize(eventType.EffectValue, eventType.EffectValue2);
        }

        public void SetCurrentMaterial(Material material)
        {
            currentMaterial = material;
            if (meshRenderer != null)
                meshRenderer.material = currentMaterial;
        }

        public override void UseTool(RaycastHit hit)
        {
            lastHit = hit;
            PerformToolAction();
        }

        public override void PerformToolAction()
        {
            var detectedTextureIndex = terrainLayerDetector.GetTextureIndex(lastHit, out _);
// First, see if it's a mesh or rock object we can mine
            var isMinableObject = CanInteractWithObject(lastHit.collider.gameObject);

// Then decide if it’s terrain and valid
            var isTerrain = detectedTextureIndex >= 0;
            var isValidTerrain = CanInteractWithTextureIndex(detectedTextureIndex);

            if (!isMinableObject && (!isTerrain || !isValidTerrain)) return;

            var textureIndex = GetTerrainLayerBasedOnDepthAndOverrides(detectedTextureIndex, lastHit.point.y);


            if (Time.time < lastDigTime + miningCooldown)
                return;

            lastDigTime = Time.time;

            if (playerInteraction == null || digger == null)
                return;


            var notPlayerMask = ~playerInteraction.playerLayerMask;
            if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit,
                    diggerUsingRange,
                    notPlayerMask))
                return;

            // Cache hit for external access
            lastHit = hit;

            // Interact
            if (CanInteractWithObject(hit.collider.gameObject))
            {
                // Call IInteractable if implemented
                hit.collider.GetComponent<IInteractable>()?.Interact();


                var minable = hit.collider.GetComponent<IMinable>();
                if (minable != null)
                {
                    if (minable.GetCurrentMinableHardness() <= hardnessCanBreak)
                    {
                        minable.MinableMineHit();
                        moveToolDespiteFailHitFeedbacks?.PlayFeedbacks();
                    }
                    else
                    {
                        minable.MinableFailHit(hit.point);
                        moveToolDespiteFailHitFeedbacks?.PlayFeedbacks();
                    }
                }
            }

            // Return after triggering failed mining feedbacks, and before digging
            if (!allowedTerrainTextureIndices.Contains(textureIndex)) return;


            // Distance check: is this close enough to the last hit?
            if (terrainHitCount > 0 && Vector3.Distance(hit.point, lastHitPosition) > hitThresholdDistance)
                terrainHitCount = 0;

            // Store current hit
            lastHitPosition = hit.point;
            terrainHitCount++;

            if (terrainHitCount < 2)
            {
                var digPositionFirst = hit.point + mainCamera.transform.forward * 0.3f;

                StartCoroutine(Dig(digPositionFirst, textureIndex, firstHitEffectOpacity,
                    firstHitEffectRadius, BrushType.Stalagmite)); // first hit dig
                TriggerDebrisEffect(debrisEffectFirstHitPrefab, hit);
                crackSpawner.ApplyDecal();

                firstHitFeedbacks?.PlayFeedbacks(hit.point); // optional first-hit feedback

                return;
            }

            if (terrainHitCount == 2)
            {
                UnityEngine.Debug.Log("Removing decal");
                TriggerDebrisEffect(debrisEffectSecondHitPrefab, hit);
                secondHitFeedbacks?.PlayFeedbacks(hit.point); // optional second-hit feedback
                crackSpawner.RemoveDecal();
            }


            terrainHitCount = 0;

            var digPosition = hit.point + mainCamera.transform.forward * 0.3f;

            StartCoroutine(Dig(digPosition, textureIndex, effectOpacity, effectRadius));
        }

        private IEnumerator Dig(Vector3 digPosition, int textureIndex,
            float effectOpacityLoc, float effectRadiusLoc, BrushType brushLoc = BrushType.Sphere)
        {
            {
                yield return new WaitForSeconds(delayBeforeDigging);
                if (EditAsynchronously)
                    digger.ModifyAsyncBuffured(digPosition, brushLoc, Action, textureIndex, effectOpacity, effectRadius,
                        stalagmiteHeight, true);
                else
                    digger.Modify(digPosition, brushLoc, Action, textureIndex, effectOpacity, effectRadius,
                        stalagmiteHeight,
                        true);

                // FuelEvent.Trigger(FuelEventType.ConsumeFuel, 2f, PlayerFuelManager.MaxFuelPoints);
            }
        }
    }
}