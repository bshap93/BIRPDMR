using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Input.Scripts;
using Domains.Player.Events;
using MoreMountains.Feedbacks;
using ThirdParty.Character_Controller_Pro.Implementation.Scripts.Character.States;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public abstract class MiningState : CharacterState
    {
        [SerializeField] protected float miningRange = 5f;
        [SerializeField] protected GameObject effectPrefab; // Generic effect prefab
        public Transform cameraTransform;
        public float currentDigDepth;


        // Shared mining properties
        [Header("Mining Parameters")] public float staminaExpense = 2f;


        // Layer depth thresholds (where material changes occur)
        public float[] layerDepthThresholds = { 10f, 20f, 35f }; // Dirt, stone, deeper materials

        // Feedback effects
        [SerializeField] protected MMFeedbacks miningBehavior;
        [SerializeField] protected MMFeedbacks outOfStaminaFeedback;

        // Digger parameters
        protected DiggerMasterRuntime _diggerMasterRuntime;
        protected PlayerInteraction playerInteraction;

        protected override void Awake()
        {
            base.Awake();

            // Find references
            _diggerMasterRuntime = FindFirstObjectByType<DiggerMasterRuntime>();
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        public override bool CheckEnterTransition(CharacterState fromState)
        {
            if (!PlayerStaminaManager.IsPlayerOutOfStamina()) return true;

            outOfStaminaFeedback?.PlayFeedbacks();
            return false;
        }


        // This is the required abstract method implementation
        public override void UpdateBehaviour(float dt)
        {
            // Check for mining input
            if (CustomInputBindings.IsMineMouseButtonPressed())
            {
                // Perform the actual mining - call the concrete implementation
                PerformMining();

                // Play mining feedback
                miningBehavior?.PlayFeedbacks();
            }
        }

        // Abstract method to be implemented by specific tool types
        public abstract void PerformMining();

        // Core mining implementation - can be used by derived classes
        protected virtual void PerformMiningCore(RaycastHit hit)
        {
            // Track digging depth and update environment effects
            UpdateDepthEffects(hit.point);

            // Update light effects based on depth

            // Spawn effects at impact point
            SpawnMiningEffect(hit);

            // Apply the digging operation
            var strokeStart = hit.point;
            var strokeDirection = cameraTransform.forward * 0.3f;

            // Get the appropriate texture index based on depth
            var textureIndex = GetLayerTextureIndexFromDepth(4000f);


            // Modify terrain
            ModifyTerrain(strokeStart, strokeDirection, textureIndex);

            // Consume stamina
            StaminaEvent.Trigger(StaminaEventType.ConsumeStamina, staminaExpense);
        }

        // Methods to be overridden by specific tools
        protected virtual void SpawnMiningEffect(RaycastHit hit)
        {
            if (effectPrefab == null) return;

            var spawnPosition = hit.point + hit.normal * 0.05f;
            var effect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);
            effect.transform.rotation = Quaternion.LookRotation(hit.normal);
        }

        protected virtual void ModifyTerrain(Vector3 position, Vector3 direction, int textureIndex)
        {
            // Default implementation - must be overridden in derived classes
        }

        protected virtual void UpdateDepthEffects(Vector3 digPosition)
        {
            currentDigDepth = digPosition.y;
        }

        protected virtual int GetLayerTextureIndexFromDepth(float depth)
        {
            return 1;
        }

        public override void CheckExitTransition()
        {
            if (PlayerStaminaManager.IsPlayerOutOfStamina())
            {
                outOfStaminaFeedback?.PlayFeedbacks();
                CharacterStateController.EnqueueTransition<MyNormalMovement>();
            }

            if (!CustomInputBindings.IsMineMouseButtonPressed())
                CharacterStateController.EnqueueTransition<MyNormalMovement>();
        }
    }
}