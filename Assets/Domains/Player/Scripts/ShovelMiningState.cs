using Digger.Modules.Core.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Player.Scripts
{
    public class ShovelMiningState : MiningState, MMEventListener<UpgradeEvent>
    {
        [FormerlySerializedAs("opacity")] public float shovelToolEffectOpacity;
        public BrushType brush = BrushType.Stalagmite;
        public ActionType action = ActionType.Dig;
        public float stalagmiteHeight = 10F;
        public bool editAsynchronously = true;

        [SerializeField] private GameObject debrisEffectPrefab;


        [FormerlySerializedAs("size")] [SerializeField]
        private float shovelToolEffectRadius;

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(UpgradeEvent eventType)
        {
            if (eventType.EventType == UpgradeEventType.ShovelMiningSizeSet)
                SetShovelEffectSize(eventType.EffectValue, eventType.EffectValue2);
        }

        public float GetSize()
        {
            return shovelToolEffectRadius;
        }

        protected override void ModifyTerrain(Vector3 position, Vector3 direction, int textureIndex)
        {
            var strokePosition = position + direction;

            if (editAsynchronously)
                _diggerMasterRuntime.ModifyAsyncBuffured(
                    strokePosition, brush, action, textureIndex, shovelToolEffectOpacity, shovelToolEffectRadius,
                    stalagmiteHeight);
            else
                _diggerMasterRuntime.Modify(
                    strokePosition, brush, action, textureIndex, shovelToolEffectOpacity, shovelToolEffectRadius);
        }

        // Implementation of the main mining loop
        public override void PerformMining()
        {
            if (playerInteraction == null) return;

            var notPlayerMask = ~playerInteraction.playerLayerMask;


            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, miningRange, notPlayerMask))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null) interactable.Interact();

                if (Camera.main != null)
                {
                    var digPoint = Camera.main.transform.position + Camera.main.transform.forward * 2f;
                    var rotation = Quaternion.LookRotation(-Camera.main.transform.forward); // optional, face player

                    if (debrisEffectPrefab != null)
                    {
                        var debris = Instantiate(debrisEffectPrefab, digPoint, rotation);
                        Destroy(debris, 2f); // destroy after 2 seconds
                    }
                }

                // Use the base class implementation for core mining functionality
                PerformMiningCore(hit);
            }
        }

        public void SetShovelEffectSize(float newEffectRadius, float newEffectOpacity)
        {
            // Apply safety limits
            var minSize = 0.4f;
            var maxSize = 1.2f;

            // Validate and apply size
            shovelToolEffectRadius = Mathf.Clamp(newEffectRadius, minSize, maxSize);
            shovelToolEffectOpacity = Mathf.Clamp(newEffectOpacity, 5f, 15f);

            // Log the assigned size for debugging
            UnityEngine.Debug.Log($"ShovelMiningState.size set to: {shovelToolEffectRadius}");
        }

        public override void UpdateBehaviour(float dt)
        {
            miningBehavior?.PlayFeedbacks();
        }
    }
}