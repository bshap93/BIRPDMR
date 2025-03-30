using Digger.Modules.Core.Sources;
using Domains.Gameplay.Mining.Scripts;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public class ShovelMiningState : MiningState
    {
        [Header("Shovel-Specific Parameters")] public int strokeCount = 1;
        public float opacity = 1f;
        public BrushType brush = BrushType.Stalagmite;
        public ActionType action = ActionType.Dig;
        public float stalagmiteHeight = 10F;
        public bool editAsynchronously = true;

        [SerializeField] private GameObject debrisEffectPrefab;


        private float size;

        public float GetSize()
        {
            return size;
        }

        protected override void ModifyTerrain(Vector3 position, Vector3 direction, int textureIndex)
        {
            for (var i = 0; i < strokeCount; i++)
            {
                var strokePosition = position + direction * i;

                if (editAsynchronously)
                    _diggerMasterRuntime.ModifyAsyncBuffured(
                        strokePosition, brush, action, textureIndex, opacity, size, stalagmiteHeight);
                else
                    _diggerMasterRuntime.Modify(
                        strokePosition, brush, action, textureIndex, opacity, size);
            }
        }

        // Implementation of the main mining loop
        public override void PerformMining()
        {
            if (playerInteraction == null) return;

            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, miningRange))
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

        public void SetMiningSize(float newSize)
        {
            // Apply safety limits
            var minSize = 0.1f;
            var maxSize = 0.8f;

            // Validate and apply size
            size = Mathf.Clamp(newSize, minSize, maxSize);

            // Log the assigned size for debugging
            UnityEngine.Debug.Log($"ShovelMiningState.size set to: {size}");
        }

        public override void UpdateBehaviour(float dt)
        {
            miningBehavior?.PlayFeedbacks();
        }
    }
}