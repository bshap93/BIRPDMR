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
        private DiggerMasterRuntime _diggerMasterRuntime;

        public ToolIteration toolIteration;


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
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, miningRange))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null) interactable.Interact();

                var strokeStart = hit.point;
                var strokeDirection = cameraTransform.forward * 0.3f; // Moves strokes forward

                var miningPerformed = false; // ✅ Track if mining actually happened

                for (var i = 0; i < strokeCount; i++)
                {
                    var strokePosition = strokeStart + strokeDirection * i;

                    if (editAsynchronously)
                        _diggerMasterRuntime.ModifyAsyncBuffured(strokePosition, brush, action, textureIndex, opacity,
                            size, stalagmiteHeight);
                    else
                        _diggerMasterRuntime.Modify(strokePosition, brush, action, textureIndex, opacity, size);

                    miningPerformed = true; // ✅ Mark that mining actually occurred
                }

                if (miningPerformed) // ✅ Play feedback ONLY ONCE
                    StaminaEvent.Trigger(StaminaEventType.ConsumeStamina, staminaExpense);
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
    }
}