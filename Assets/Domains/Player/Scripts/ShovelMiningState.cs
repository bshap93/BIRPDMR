using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Input.Scripts;
using Domains.Player.Events;
using Lightbug.CharacterControllerPro.Implementation;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Player.Scripts
{
    public class ShovelMiningState : CharacterState
    {
        [SerializeField] private float miningRange = 5f;
        public Animator toolAnimator;
        public Transform cameraTransform;
        public int strokeCount = 1;
        
        [SerializeField] private GameObject dirtParticlePrefab;



        [FormerlySerializedAs("miningBehabior")]
        public MMFeedbacks miningBehavior;

        public float staminaExpense = 2f;

        [FormerlySerializedAs("cannotMineFeedbacks")]
        public MMFeedbacks playerIsOutOfStaminaFB;

        // Digger parameters
        public float size;
        public float opacity;
        public BrushType brush = BrushType.Stalagmite;
        public ActionType action = ActionType.Dig;
        [FormerlySerializedAs("bruchScale")] public Vector3 brushScale = new(1.5f, 0.5f, 0.5f); // Wider strokes
        public int textureIndex;
        public float stalagmiteHeight = 10F;

        public bool editAsynchronously = true;

        public ToolIteration toolIteration;
        private DiggerMasterRuntime _diggerMasterRuntime;

        private PlayerInteraction playerInteraction;


        protected override void Start()
        {
            base.Start();
            _diggerMasterRuntime = FindFirstObjectByType<DiggerMasterRuntime>();
            if (!_diggerMasterRuntime)
            {
                UnityEngine.Debug.LogWarning(
                    "DiggerRuntimeUsageExample component requires DiggerMasterRuntime component to be setup in the scene. DiggerRuntimeUsageExample will be disabled.");

                enabled = false;
            }

            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        // Write your transitions here
        public override bool CheckEnterTransition(CharacterState fromState)
        {
            if (!PlayerStaminaManager.IsPlayerOutOfStamina()) return true;

            playerIsOutOfStaminaFB?.PlayFeedbacks();
            return false;
        }


        public override void UpdateBehaviour(float dt)
        {
            miningBehavior?.PlayFeedbacks();
        }

        public void PerformMining()
        {
            if (playerInteraction == null)
                return;

            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, miningRange))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null) interactable.Interact();

                // This is the exact spot you're looking at/targeting
                var impactPoint = hit.point;
        
// In your PerformMining method, modify the particle instantiation code:

// Spawn dirt particle effect at the impact point
                if (dirtParticlePrefab != null)
                {
                    // Offset the position slightly away from the surface to prevent clipping
                    var spawnPosition = impactPoint + hit.normal * 0.05f;
    
                    GameObject effect = Instantiate(dirtParticlePrefab, spawnPosition, Quaternion.identity);
    
                    // Orient particles to face outward from the surface
                    // Change this to ensure particles emit away from the surface, not toward the camera
                    effect.transform.rotation = Quaternion.LookRotation(hit.normal);
    
                    // Check if particle system exists and isn't auto-playing
                    ParticleSystem system = effect.GetComponent<ParticleSystem>();
                    if (system != null && !system.main.playOnAwake)
                    {
                        system.Play();
                    }
    
                    // Debug log to see where particles are spawning
                    UnityEngine.Debug.Log($"Spawning particles at {spawnPosition}, normal: {hit.normal}, camera pos: {cameraTransform.position}");
                }

                // Continue with existing digging code
                var strokeStart = hit.point;
                var strokeDirection = cameraTransform.forward * 0.3f;

                for (var i = 0; i < strokeCount; i++)
                {
                    var strokePosition = strokeStart + strokeDirection * i;

                    if (editAsynchronously)
                        _diggerMasterRuntime.ModifyAsyncBuffured(
                            strokePosition, brush, action, textureIndex, opacity, size, stalagmiteHeight);
                    else
                        _diggerMasterRuntime.Modify(
                            strokePosition, brush, action, textureIndex, opacity, size);
                }

                StaminaEvent.Trigger(StaminaEventType.ConsumeStamina, staminaExpense);
                miningBehavior?.PlayFeedbacks();
            }
        }


        // Write your transitions here
        public override void CheckExitTransition()
        {
            if (PlayerStaminaManager.IsPlayerOutOfStamina())
            {
                playerIsOutOfStaminaFB?.PlayFeedbacks();
                CharacterStateController.EnqueueTransition<MyNormalMovement>();
            }

            if (!CustomInputBindings.IsMineMouseButtonPressed())
                CharacterStateController.EnqueueTransition<MyNormalMovement>();
        }

        public void SetMiningSize(float newSize)
        {
            size = newSize;
        }
    }
}