using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Input.Scripts;
using Domains.Player.Events;
using Lightbug.CharacterControllerPro.Implementation;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public class ShovelMiningState : CharacterState
    {
        [SerializeField] private float miningRange = 5f;
        public Animator toolAnimator;
        public Transform cameraTransform;
        public int strokeCount = 1;

        public float staminaExpense = 2f;

        public MMFeedbacks cannotMineFeedbacks;

        // Digger parameters
        public float size;
        public float opacity;
        public BrushType brush = BrushType.Sphere;
        public ActionType action = ActionType.Dig;
        public Vector3 bruchScale = new(1.5f, 0.5f, 0.5f); // Wider strokes
        public int textureIndex;

        public bool editAsynchronously = true;
        private DiggerMasterRuntime _diggerMasterRuntime;


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
            // if (toolAnimator != null) toolAnimator.SetBool(SwingMiningTool, true);
            if (!PlayerStaminaManager.IsPlayerOutOfStamina()) return PerformMining();

            cannotMineFeedbacks?.PlayFeedbacks();
            return false;
        }

        private float miningCooldown = 0.4f; // Adjust timing as needed
        private float nextMiningTime = 0f;

        public override void UpdateBehaviour(float dt)
        {
            if (Time.time >= nextMiningTime)
                if (PerformMining())
                    nextMiningTime = Time.time + miningCooldown; // Prevent rapid multiple triggers
        }

        private bool PerformMining()
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
                            size);
                    else
                        _diggerMasterRuntime.Modify(strokePosition, brush, action, textureIndex, opacity, size);

                    miningPerformed = true; // ✅ Mark that mining actually occurred
                }

                if (miningPerformed) // ✅ Play feedback ONLY ONCE
                    StaminaEvent.Trigger(StaminaEventType.ConsumeStamina, staminaExpense);

                return miningPerformed;
            }

            return false;
        }


        // Write your transitions here
        public override void CheckExitTransition()
        {
            if (PlayerStaminaManager.IsPlayerOutOfStamina())
            {
                cannotMineFeedbacks?.PlayFeedbacks();
                CharacterStateController.EnqueueTransition<MyNormalMovement>();
            }

            if (!CustomInputBindings.IsMineMouseButtonPressed())
                CharacterStateController.EnqueueTransition<MyNormalMovement>();
        }
    }
}