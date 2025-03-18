using System;
using Domains.Input.Scripts;
using Domains.Mining.Scripts;
using Domains.Player.Events;
using Domains.Player.Scripts;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.Utilities;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Gameplay.Mining.Scripts
{
    [MMRequiresConstantRepaint]
    [AddComponentMenu("Character Controller Pro/Demo/Character/States/Normal Movement")]
    public class MyNormalMovement : CharacterState
    {
        public enum JumpResult
        {
            Invalid,
            Grounded,
            NotGrounded
        }

        [Space(10)] public PlanarMovementParameters planarMovementParameters = new();

        public VerticalMovementParameters verticalMovementParameters = new();

        public CrouchParameters crouchParameters = new();

        public LookingDirectionParameters lookingDirectionParameters = new();

        [Header("Feedbacks")] [SerializeField] private MMFeedbacks mineFeedback;


        [Header("Animation")] [SerializeField] protected string groundedParameter = "Grounded";

        [SerializeField] protected string stableParameter = "Stable";

        [SerializeField] protected string verticalSpeedParameter = "VerticalSpeed";

        [SerializeField] protected string planarSpeedParameter = "PlanarSpeed";

        [SerializeField] protected string horizontalAxisParameter = "HorizontalAxis";

        [SerializeField] protected string verticalAxisParameter = "VerticalAxis";

        [SerializeField] protected string heightParameter = "Height";

        [Header("Jet Pack")] [SerializeField] protected float jetPackDuration = 0.1f;

        [SerializeField] protected float jetPackSpeedMultiplier = 3f;

        // Add these new fields for fall damage
        [Header("Fall Damage")] [SerializeField]
        protected bool enableFallDamage = true;

        [SerializeField] protected float minimumFallDamageSpeed = 10f;
        [SerializeField] protected float fallDamageMultiplier = 1f;


        protected PlanarMovementParameters.PlanarMovementProperties currentMotion;
        protected float currentPlanarSpeedLimit;

        protected bool groundedJumpAvailable;
        protected bool isAllowedToCancelJump;
        protected bool isCrouched;
        protected bool isFalling;
        protected Vector3 jumpDirection;


        protected MaterialController materialController;

        // Track the maximum fall velocity for damage calculation
        protected float maxFallSpeed;
        protected int notGroundedJumpsLeft;
        private bool reducedAirControlFlag;
        private float reducedAirControlInitialTime;
        private float reductionDuration = 0.5f;
        protected Vector3 smoothDampVelocity = Vector3.zero;
        protected float targetHeight = 1f;

        protected Vector3 targetLookingDirection;

        protected bool wantToCrouch;
        protected bool wantToRun;

        /// <summary>
        ///     Gets/Sets the useGravity toggle. Use this property to enable/disable the effect of gravity on the character.
        /// </summary>
        /// <value></value>
        public bool UseGravity
        {
            get => verticalMovementParameters.useGravity;
            set => verticalMovementParameters.useGravity = value;
        }


        protected bool UnstableGroundedJumpAvailable => !verticalMovementParameters.canJumpOnUnstableGround &&
                                                        CharacterActor.CurrentState ==
                                                        CharacterActorState.UnstableGrounded;

        protected override void Awake()
        {
            base.Awake();

            notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;

            materialController = this.GetComponentInBranch<CharacterActor, MaterialController>();
        }

        protected override void Start()
        {
            base.Start();

            targetHeight = CharacterActor.DefaultBodySize.y;

            var minCrouchHeightRatio = CharacterActor.BodySize.x / CharacterActor.BodySize.y;
            crouchParameters.heightRatio = Mathf.Max(minCrouchHeightRatio, crouchParameters.heightRatio);
        }

        protected virtual void OnEnable()
        {
            CharacterActor.OnTeleport += OnTeleport;
        }

        protected virtual void OnDisable()
        {
            CharacterActor.OnTeleport -= OnTeleport;
        }

        protected virtual void OnValidate()
        {
            verticalMovementParameters.OnValidate();
        }

        public override string GetInfo()
        {
            return
                "This state serves as a multi purpose movement based state. It is responsible for handling gravity and jump, walk and run, crouch, " +
                "react to the different material properties, etc. Basically it covers all the common movements involved " +
                "in a typical game, from a 3D platformer to a first person walking simulator.";
        }

        private void OnTeleport(Vector3 position, Quaternion rotation)
        {
            targetLookingDirection = CharacterActor.Forward;
            isAllowedToCancelJump = false;

            // Reset fall tracking when teleporting
            isFalling = false;
            maxFallSpeed = 0f;
        }

        public override void CheckExitTransition()
        {
            if (CustomInputBindings.IsMineMouseButtonPressed() && !PlayerStaminaManager.IsPlayerOutOfStamina())
            {
                mineFeedback?.PlayFeedbacks();
                UnityEngine.Debug.Log("Mining");

                CharacterStateController.EnqueueTransition<ShovelMiningState>();

                mineFeedback?.PlayFeedbacks();
            }
        }

        public override void ExitBehaviour(float dt, CharacterState toState)
        {
            reducedAirControlFlag = false;
        }


        /// <summary>
        ///     Reduces the amount of acceleration and deceleration (not grounded state) until the character reaches the apex of
        ///     the jump
        ///     (vertical velocity close to zero). This can be useful to prevent the character from accelerating/decelerating too
        ///     quickly (e.g. right after performing a wall jump).
        /// </summary>
        public void ReduceAirControl(float reductionDuration = 0.5f)
        {
            reducedAirControlFlag = true;
            reducedAirControlInitialTime = Time.time;
            this.reductionDuration = reductionDuration;
        }

        private void SetMotionValues(Vector3 targetPlanarVelocity)
        {
            var angleCurrentTargetVelocity = Vector3.Angle(CharacterActor.PlanarVelocity, targetPlanarVelocity);

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:

                    currentMotion.acceleration = planarMovementParameters.stableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.stableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier =
                        planarMovementParameters.stableGroundedAngleAccelerationBoost.Evaluate(
                            angleCurrentTargetVelocity);

                    break;

                case CharacterActorState.UnstableGrounded:
                    currentMotion.acceleration = planarMovementParameters.unstableGroundedAcceleration;
                    currentMotion.deceleration = planarMovementParameters.unstableGroundedDeceleration;
                    currentMotion.angleAccelerationMultiplier =
                        planarMovementParameters.unstableGroundedAngleAccelerationBoost.Evaluate(
                            angleCurrentTargetVelocity);

                    break;

                case CharacterActorState.NotGrounded:

                    if (reducedAirControlFlag)
                    {
                        var time = Time.time - reducedAirControlInitialTime;
                        if (time <= reductionDuration)
                        {
                            currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration /
                                reductionDuration * time;

                            currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration /
                                reductionDuration * time;
                        }
                        else
                        {
                            reducedAirControlFlag = false;

                            currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                            currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                        }
                    }
                    else
                    {
                        currentMotion.acceleration = planarMovementParameters.notGroundedAcceleration;
                        currentMotion.deceleration = planarMovementParameters.notGroundedDeceleration;
                    }

                    currentMotion.angleAccelerationMultiplier =
                        planarMovementParameters.notGroundedAngleAccelerationBoost.Evaluate(angleCurrentTargetVelocity);

                    break;
            }


            // Material values
            if (materialController != null)
            {
                if (CharacterActor.IsGrounded)
                {
                    currentMotion.acceleration *= materialController.CurrentSurface.accelerationMultiplier *
                                                  materialController.CurrentVolume.accelerationMultiplier;

                    currentMotion.deceleration *= materialController.CurrentSurface.decelerationMultiplier *
                                                  materialController.CurrentVolume.decelerationMultiplier;
                }
                else
                {
                    currentMotion.acceleration *= materialController.CurrentVolume.accelerationMultiplier;
                    currentMotion.deceleration *= materialController.CurrentVolume.decelerationMultiplier;
                }
            }
        }


        /// <summary>
        ///     Processes the lateral movement of the character (stable and unstable state), that is, walk, run, crouch, etc.
        ///     This movement is tied directly to the "movement" character action.
        /// </summary>
        protected virtual void ProcessPlanarMovement(float dt)
        {
            //SetMotionValues();

            var speedMultiplier = materialController != null
                ? materialController.CurrentSurface.speedMultiplier * materialController.CurrentVolume.speedMultiplier
                : 1f;


            var needToAccelerate =
                CustomUtilities.Multiply(CharacterStateController.InputMovementReference, currentPlanarSpeedLimit)
                    .sqrMagnitude >= CharacterActor.PlanarVelocity.sqrMagnitude;

            Vector3 targetPlanarVelocity = default;
            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.NotGrounded:

                    if (CharacterActor.WasGrounded)
                        currentPlanarSpeedLimit = Mathf.Max(
                            CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

                    targetPlanarVelocity = CustomUtilities.Multiply(
                        CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.StableGrounded:


                    // Run ------------------------------------------------------------
                    if (planarMovementParameters.runInputMode == InputMode.Toggle)
                    {
                        if (CharacterActions.run.Started)
                            wantToRun = !wantToRun;
                    }
                    else
                    {
                        wantToRun = CharacterActions.run.value;
                    }

                    if (wantToCrouch || !planarMovementParameters.canRun)
                        wantToRun = false;


                    if (isCrouched)
                        currentPlanarSpeedLimit =
                            planarMovementParameters.baseSpeedLimit * crouchParameters.speedMultiplier;
                    else
                        currentPlanarSpeedLimit = wantToRun
                            ? planarMovementParameters.boostSpeedLimit
                            : planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply(
                        CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);

                    break;
                case CharacterActorState.UnstableGrounded:

                    currentPlanarSpeedLimit = planarMovementParameters.baseSpeedLimit;

                    targetPlanarVelocity = CustomUtilities.Multiply(
                        CharacterStateController.InputMovementReference, speedMultiplier, currentPlanarSpeedLimit);


                    break;
            }

            SetMotionValues(targetPlanarVelocity);


            var acceleration = currentMotion.acceleration;


            if (needToAccelerate)
                acceleration *= currentMotion.angleAccelerationMultiplier;
            else
                acceleration = currentMotion.deceleration;

            CharacterActor.PlanarVelocity = Vector3.MoveTowards(
                CharacterActor.PlanarVelocity,
                targetPlanarVelocity,
                acceleration * dt
            );
        }


        protected virtual void ProcessGravity(float dt)
        {
            if (!verticalMovementParameters.useGravity)
                return;


            verticalMovementParameters.UpdateParameters();


            var gravityMultiplier = 1f;

            if (materialController != null)
                gravityMultiplier = CharacterActor.LocalVelocity.y >= 0
                    ? materialController.CurrentVolume.gravityAscendingMultiplier
                    : materialController.CurrentVolume.gravityDescendingMultiplier;

            var gravity = gravityMultiplier * verticalMovementParameters.gravity;


            if (!CharacterActor.IsStable)
                CharacterActor.VerticalVelocity += CustomUtilities.Multiply(-CharacterActor.Up, gravity, dt);
        }

        private JumpResult CanJump()
        {
            var jumpResult = JumpResult.Invalid;

            if (!verticalMovementParameters.canJump)
                return jumpResult;

            if (isCrouched)
                return jumpResult;

            switch (CharacterActor.CurrentState)
            {
                case CharacterActorState.StableGrounded:

                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime &&
                        groundedJumpAvailable)
                        jumpResult = JumpResult.Grounded;

                    break;
                case CharacterActorState.NotGrounded:

                    if (CharacterActions.jump.Started)
                    {
                        // First check if the "grounded jump" is available. If so, execute a "coyote jump".
                        if (CharacterActor.NotGroundedTime <= verticalMovementParameters.postGroundedJumpTime &&
                            groundedJumpAvailable)
                            jumpResult = JumpResult.Grounded;
                        else if (notGroundedJumpsLeft != 0) // Do a 'not grounded' jump
                            jumpResult = JumpResult.NotGrounded;
                    }

                    break;
                case CharacterActorState.UnstableGrounded:

                    if (CharacterActions.jump.StartedElapsedTime <= verticalMovementParameters.preGroundedJumpTime &&
                        verticalMovementParameters.canJumpOnUnstableGround)
                        jumpResult = JumpResult.Grounded;

                    break;
            }

            return jumpResult;
        }


        protected virtual void ProcessJump(float dt)
        {
            ProcessRegularJump(dt);
            ProcessJumpDown(dt);
        }

        protected virtual void ProcessJetPack(float dt)
        {
            if (CharacterActions.jetPack.value)
                CharacterActor.VerticalVelocity = Vector3.SmoothDamp(
                    CharacterActor.VerticalVelocity,
                    targetHeight * CharacterActor.Up,
                    ref smoothDampVelocity,
                    jetPackDuration
                ) * jetPackSpeedMultiplier;

            // CharacterActor.PlanarVelocity = Vector3.Lerp(CharacterActor.PlanarVelocity,
            //     planarMovementParameters.targetPlanarSpeed * CharacterStateController.InputMovementReference, 7f * dt);

            CharacterActor.SetYaw(CharacterActor.PlanarVelocity);
        }


        private void ProcessVerticalMovement(float dt)
        {
            // Track falling state and maximum fall speed
            if (enableFallDamage)
            {
                // If we're falling (negative vertical velocity in the up direction)
                var currentFallSpeed = -Vector3.Dot(CharacterActor.Velocity, CharacterActor.Up);

                if (currentFallSpeed > 0 && !CharacterActor.IsGrounded)
                {
                    isFalling = true;
                    // Track the maximum fall speed during this fall
                    maxFallSpeed = Mathf.Max(maxFallSpeed, currentFallSpeed);
                }

                // Check for landing impact
                if (isFalling && CharacterActor.IsGrounded)
                {
                    HandleFallDamage();
                    // Reset fall tracking
                    isFalling = false;
                    maxFallSpeed = 0f;
                }
            }

            ProcessGravity(dt);
            ProcessJump(dt);
            ProcessJetPack(dt);
        }

        // New method to handle fall damage
        protected virtual void HandleFallDamage()
        {
            if (!enableFallDamage)
                return;

            // If fall speed exceeds the threshold, apply damage
            if (maxFallSpeed > minimumFallDamageSpeed)
            {
                // Calculate damage - simple linear model based on speed beyond the threshold
                var excessSpeed = maxFallSpeed - minimumFallDamageSpeed;
                var damage = excessSpeed * fallDamageMultiplier;

                // Call the damage event
                UnityEngine.Debug.Log("Fall damage applied: " + damage);
                HealthEvent.Trigger(HealthEventType.ConsumeHealth, damage);

                // Optional: Debug output
                UnityEngine.Debug.Log($"Fall damage applied: {damage} from fall speed: {maxFallSpeed}");
            }
        }


        public override void EnterBehaviour(float dt, CharacterState fromState)
        {
            targetLookingDirection = CharacterActor.Forward;
            CharacterActor.alwaysNotGrounded = false;

            // Reset fall tracking
            isFalling = false;
            maxFallSpeed = 0f;

            // Grounded jump
            groundedJumpAvailable = false;
            if (CharacterActor.IsGrounded)
                if (verticalMovementParameters.canJumpOnUnstableGround || CharacterActor.IsStable)
                    groundedJumpAvailable = true;

            // Wallside to NormalMovement transition
            if (fromState == CharacterStateController.GetState<WallSlide>())
            {
                // "availableNotGroundedJumps + 1" because the update code will consume one jump!
                notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps + 1;

                // Reduce the amount of air control (acceleration and deceleration) for 0.5 seconds.
                ReduceAirControl();
            }

            currentPlanarSpeedLimit = Mathf.Max(
                CharacterActor.PlanarVelocity.magnitude, planarMovementParameters.baseSpeedLimit);

            CharacterActor.UseRootMotion = false;
        }

        protected virtual void HandleRotation(float dt)
        {
            HandleLookingDirection(dt);
        }

        private void HandleLookingDirection(float dt)
        {
            if (!lookingDirectionParameters.changeLookingDirection)
                return;

            switch (lookingDirectionParameters.lookingDirectionMode)
            {
                case LookingDirectionParameters.LookingDirectionMode.Movement:

                    switch (CharacterActor.CurrentState)
                    {
                        case CharacterActorState.NotGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.notGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.StableGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.stableGroundedLookingDirectionMode);

                            break;
                        case CharacterActorState.UnstableGrounded:

                            SetTargetLookingDirection(lookingDirectionParameters.unstableGroundedLookingDirectionMode);

                            break;
                    }

                    break;

                case LookingDirectionParameters.LookingDirectionMode.ExternalReference:

                    if (!CharacterActor.CharacterBody.Is2D)
                        targetLookingDirection = CharacterStateController.MovementReferenceForward;

                    break;

                case LookingDirectionParameters.LookingDirectionMode.Target:

                    targetLookingDirection = lookingDirectionParameters.target.position - CharacterActor.Position;
                    targetLookingDirection.Normalize();

                    break;
            }

            var targetDeltaRotation = Quaternion.FromToRotation(CharacterActor.Forward, targetLookingDirection);
            var currentDeltaRotation = Quaternion.Slerp(
                Quaternion.identity, targetDeltaRotation, lookingDirectionParameters.speed * dt);

            if (CharacterActor.CharacterBody.Is2D)
                CharacterActor.SetYaw(targetLookingDirection);
            else
                CharacterActor.SetYaw(currentDeltaRotation * CharacterActor.Forward);
        }

        private void SetTargetLookingDirection(
            LookingDirectionParameters.LookingDirectionMovementSource lookingDirectionMode)
        {
            if (lookingDirectionMode == LookingDirectionParameters.LookingDirectionMovementSource.Input)
            {
                if (CharacterStateController.InputMovementReference != Vector3.zero)
                    targetLookingDirection = CharacterStateController.InputMovementReference;
                else
                    targetLookingDirection = CharacterActor.Forward;
            }
            else
            {
                if (CharacterActor.PlanarVelocity != Vector3.zero)
                    targetLookingDirection = Vector3.ProjectOnPlane(CharacterActor.PlanarVelocity, CharacterActor.Up);
                else
                    targetLookingDirection = CharacterActor.Forward;
            }
        }

        public override void UpdateBehaviour(float dt)
        {
            HandleSize(dt);
            HandleVelocity(dt);
            HandleRotation(dt);
        }


        public override void PreCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            if (!CharacterActor.IsAnimatorValid())
                return;

            CharacterStateController.Animator.SetBool(groundedParameter, CharacterActor.IsGrounded);
            CharacterStateController.Animator.SetBool(stableParameter, CharacterActor.IsStable);
            CharacterStateController.Animator.SetFloat(horizontalAxisParameter, CharacterActions.movement.value.x);
            CharacterStateController.Animator.SetFloat(verticalAxisParameter, CharacterActions.movement.value.y);
            CharacterStateController.Animator.SetFloat(heightParameter, CharacterActor.BodySize.y);
        }

        public override void PostCharacterSimulation(float dt)
        {
            // Pre/PostCharacterSimulation methods are useful to update all the Animator parameters. 
            // Why? Because the CharacterActor component will end up modifying the velocity of the actor.
            if (!CharacterActor.IsAnimatorValid())
                return;

            // Parameters associated with velocity are sent after the simulation.
            // The PostSimulationUpdate (CharacterActor) might update velocity once more (e.g. if a "bad step" has been detected).
            CharacterStateController.Animator.SetFloat(verticalSpeedParameter, CharacterActor.LocalVelocity.y);
            CharacterStateController.Animator.SetFloat(planarSpeedParameter, CharacterActor.PlanarVelocity.magnitude);
        }

        protected virtual void HandleSize(float dt)
        {
            // Get the crouch input state 
            if (crouchParameters.enableCrouch)
            {
                if (crouchParameters.inputMode == InputMode.Toggle)
                {
                    if (CharacterActions.crouch.Started)
                        wantToCrouch = !wantToCrouch;
                }
                else
                {
                    wantToCrouch = CharacterActions.crouch.value;
                }

                if (!crouchParameters.notGroundedCrouch && !CharacterActor.IsGrounded)
                    wantToCrouch = false;

                if (CharacterActor.IsGrounded && wantToRun)
                    wantToCrouch = false;
            }
            else
            {
                wantToCrouch = false;
            }

            if (wantToCrouch)
                Crouch(dt);
            else
                StandUp(dt);
        }

        private void Crouch(float dt)
        {
            var sizeReferenceType = CharacterActor.IsGrounded
                ? CharacterActor.SizeReferenceType.Bottom
                : crouchParameters.notGroundedReference;

            var validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y * crouchParameters.heightRatio,
                crouchParameters.sizeLerpSpeed * dt,
                sizeReferenceType);

            if (validSize)
                isCrouched = true;
        }

        private void StandUp(float dt)
        {
            var sizeReferenceType = CharacterActor.IsGrounded
                ? CharacterActor.SizeReferenceType.Bottom
                : crouchParameters.notGroundedReference;

            var validSize = CharacterActor.CheckAndInterpolateHeight(
                CharacterActor.DefaultBodySize.y,
                crouchParameters.sizeLerpSpeed * dt,
                sizeReferenceType);

            if (validSize)
                isCrouched = false;
        }


        protected virtual void HandleVelocity(float dt)
        {
            ProcessVerticalMovement(dt);
            ProcessPlanarMovement(dt);
        }


        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────


        #region Events

        /// <summary>
        ///     Event triggered when the character jumps.
        /// </summary>
        public event Action OnJumpPerformed;

        /// <summary>
        ///     Event triggered when the character jumps from the ground.
        /// </summary>
        public event Action<bool> OnGroundedJumpPerformed;

        /// <summary>
        ///     Event triggered when the character jumps while.
        /// </summary>
        public event Action<int> OnNotGroundedJumpPerformed;

        #endregion

        #region JumpDown

        protected virtual bool ProcessJumpDown(float dt)
        {
            if (!verticalMovementParameters.canJumpDown)
                return false;

            if (!CharacterActor.IsStable)
                return false;

            if (!CharacterActor.IsGroundAOneWayPlatform)
                return false;

            if (verticalMovementParameters.filterByTag)
                if (!CharacterActor.GroundObject.CompareTag(verticalMovementParameters.jumpDownTag))
                    return false;

            if (!ProcessJumpDownAction())
                return false;

            JumpDown(dt);

            return true;
        }


        protected virtual bool ProcessJumpDownAction()
        {
            return isCrouched && CharacterActions.jump.Started;
        }


        protected virtual void JumpDown(float dt)
        {
            var groundDisplacementExtraDistance = 0f;

            var groundDisplacement = CustomUtilities.Multiply(CharacterActor.GroundVelocity, dt);

            if (!CharacterActor.IsGroundAscending)
                groundDisplacementExtraDistance = groundDisplacement.magnitude;

            CharacterActor.ForceNotGrounded();

            CharacterActor.Position -=
                CustomUtilities.Multiply(
                    CharacterActor.Up,
                    CharacterConstants.ColliderMinBottomOffset + verticalMovementParameters.jumpDownDistance +
                    groundDisplacementExtraDistance
                );

            CharacterActor.VerticalVelocity -= CustomUtilities.Multiply(
                CharacterActor.Up, verticalMovementParameters.jumpDownVerticalVelocity);
        }

        #endregion

        #region Jump

        private void ResetJump()
        {
            notGroundedJumpsLeft = verticalMovementParameters.availableNotGroundedJumps;
            groundedJumpAvailable = true;
        }

        protected virtual void ProcessRegularJump(float dt)
        {
            if (CharacterActor.IsGrounded)
                if (verticalMovementParameters.canJumpOnUnstableGround || CharacterActor.IsStable)
                    ResetJump();

            if (isAllowedToCancelJump)
            {
                if (verticalMovementParameters.cancelJumpOnRelease)
                {
                    if (CharacterActions.jump.StartedElapsedTime >= verticalMovementParameters.cancelJumpMaxTime ||
                        CharacterActor.IsFalling)
                    {
                        isAllowedToCancelJump = false;
                    }
                    else if (!CharacterActions.jump.value && CharacterActions.jump.StartedElapsedTime >=
                             verticalMovementParameters.cancelJumpMinTime)
                    {
                        // Get the velocity mapped onto the current jump direction
                        var projectedJumpVelocity = Vector3.Project(CharacterActor.Velocity, jumpDirection);

                        CharacterActor.Velocity -= CustomUtilities.Multiply(
                            projectedJumpVelocity, 1f - verticalMovementParameters.cancelJumpMultiplier);

                        isAllowedToCancelJump = false;
                    }
                }
            }
            else
            {
                var jumpResult = CanJump();

                switch (jumpResult)
                {
                    case JumpResult.Grounded:
                        groundedJumpAvailable = false;

                        break;
                    case JumpResult.NotGrounded:
                        notGroundedJumpsLeft--;

                        return; // Prevents any further jump processing


                    case JumpResult.Invalid:
                        return;
                }

                // Events ---------------------------------------------------
                if (CharacterActor.IsGrounded)
                    OnGroundedJumpPerformed?.Invoke(true);
                else
                    OnNotGroundedJumpPerformed?.Invoke(notGroundedJumpsLeft);

                OnJumpPerformed?.Invoke();

                // Define the jump direction ---------------------------------------------------
                jumpDirection = SetJumpDirection();

                // Force "not grounded" state.     
                if (CharacterActor.IsGrounded)
                    CharacterActor.ForceNotGrounded();

                // First remove any velocity associated with the jump direction.
                CharacterActor.Velocity -= Vector3.Project(CharacterActor.Velocity, jumpDirection);
                CharacterActor.Velocity += CustomUtilities.Multiply(
                    jumpDirection, verticalMovementParameters.jumpSpeed);

                if (verticalMovementParameters.cancelJumpOnRelease)
                    isAllowedToCancelJump = true;
            }
        }

        /// <summary>
        ///     Returns the jump direction vector whenever the jump action is started.
        /// </summary>
        protected virtual Vector3 SetJumpDirection()
        {
            return CharacterActor.Up;
        }

        #endregion
    }
}