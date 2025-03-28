using Digger.Modules.Core.Sources;
using Domains.Gameplay.Mining.Scripts;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public class PickaxeMiningState : MiningState
    {
        [Header("Pickaxe-Specific Parameters")]
        public float size = 0.15f;

        public float opacity = 1.2f;
        public BrushType brush = BrushType.Sphere; // Different brush type for pickaxe
        public ActionType action = ActionType.Dig;
        public float rockBreakingMultiplier = 1.5f; // Pickaxe breaks stone faster

        [Header("Animation")] public Animator toolAnimator;

        [SerializeField] private GameObject rockParticlePrefab;

        protected override void SpawnMiningEffect(RaycastHit hit)
        {
            if (rockParticlePrefab != null)
            {
                var spawnPosition = hit.point + hit.normal * 0.05f;
                var effect = Instantiate(rockParticlePrefab, spawnPosition, Quaternion.identity);
                effect.transform.rotation = Quaternion.LookRotation(hit.normal);

                var system = effect.GetComponent<ParticleSystem>();
                if (system != null && !system.main.playOnAwake) system.Play();
            }
        }

        protected override void ModifyTerrain(Vector3 position, Vector3 direction, int textureIndex)
        {
            // Multiply the intensity when in stone layer
            var localOpacity = opacity;
            if (textureIndex >= 2) // Stone and deeper layers
                localOpacity *= rockBreakingMultiplier;

            _diggerMasterRuntime.ModifyAsyncBuffured(
                position, brush, action, textureIndex, localOpacity, size);
        }


        protected override int GetLayerTextureIndexFromDepth(float depth)
        {
            // Pickaxe might be better at specific layers - we could modify the logic
            return base.GetLayerTextureIndexFromDepth(depth);
        }

        public override void PerformMining()
        {
            if (playerInteraction == null) return;

            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, miningRange))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null) interactable.Interact();

                PerformMiningCore(hit);
            }
        }

        public override void UpdateBehaviour(float dt)
        {
            miningBehavior?.PlayFeedbacks();
        }
    }
}