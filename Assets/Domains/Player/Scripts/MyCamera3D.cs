using Domains.UI_Global.Events;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.Utilities;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains
{
    [AddComponentMenu("Character Controller Pro/Demo/Camera/Camera 3D")]
    [DefaultExecutionOrder(ExecutionOrder.CharacterGraphicsOrder + 100)] // <--- Do your job after everything else
    public class MyCamera3D : MonoBehaviour, MMEventListener<UIEvent>
    {
        public enum CameraMode
        {
            FirstPerson,
            ThirdPerson
        }

        [Header("Inputs")] [SerializeField] private InputHandlerSettings inputHandlerSettings = new();

        [SerializeField] private string axes = "Camera";

        [SerializeField] private string zoomAxis = "Camera Zoom";

        [Header("Target")]
        [Tooltip(
            "Select the graphics root object as your target, the one containing all the meshes, sprites, animated models, etc. \n\nImportant: This will be the considered as the actual target (visual element).")]
        [SerializeField]
        private Transform targetTransform;

        [SerializeField] private Vector3 offsetFromHead = Vector3.zero;

        [Tooltip("The interpolation speed used when the height of the character changes.")] [SerializeField]
        private float heightLerpSpeed = 10f;

        [Header("View")] public CameraMode cameraMode = CameraMode.ThirdPerson;

        [Header("First Person")] public bool hideBody = true;

        [SerializeField] private GameObject bodyObject;

        [Header("Yaw")] public bool updateYaw = true;

        public float yawSpeed = 180f;


        [Header("Pitch")] public bool updatePitch = true;

        [SerializeField] private float initialPitch = 45f;

        public float pitchSpeed = 180f;

        [Range(1f, 85f)] public float maxPitchAngle = 80f;

        [Range(1f, 85f)] public float minPitchAngle = 80f;


        [Header("Roll")] public bool updateRoll;


        [Header("Zoom (Third person)")] public bool updateZoom = true;

        [Min(0f)] [SerializeField] private float distanceToTarget = 5f;

        [Min(0f)] public float zoomInOutSpeed = 40f;

        [Min(0f)] public float zoomInOutLerpSpeed = 5f;

        [Min(0f)] public float minZoom = 2f;

        [Min(0.001f)] public float maxZoom = 12f;


        [Header("Collision")] public bool collisionDetection = true;

        public bool collisionAffectsZoom;
        public float detectionRadius = 0.5f;
        public LayerMask layerMask = 0;
        public bool considerKinematicRigidbodies = true;
        public bool considerDynamicRigidbodies = true;
        private readonly RaycastHit[] hitsBuffer = new RaycastHit[10];
        private readonly RaycastHit[] validHits = new RaycastHit[10];
        private Renderer[] bodyRenderers;

        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────


        private CharacterActor characterActor;
        private Vector3 characterPosition;
        private Rigidbody characterRigidbody;

        private float currentDistanceToTarget;
        private float deltaPitch;

        private float deltaYaw;
        private float deltaZoom;

        private Vector3 lerpedCharacterUp = Vector3.up;
        private float lerpedHeight;


        private Vector3 previousLerpedCharacterUp = Vector3.up;
        private float smoothedDistanceToTarget;

        private Transform viewReference;

        private void Awake()
        {
            Initialize(targetTransform);
        }


        private void Start()
        {
            characterPosition = targetTransform.position;

            previousLerpedCharacterUp = targetTransform.up;
            lerpedCharacterUp = previousLerpedCharacterUp;


            currentDistanceToTarget = distanceToTarget;
            smoothedDistanceToTarget = currentDistanceToTarget;

            viewReference.rotation = targetTransform.rotation;
            viewReference.Rotate(Vector3.right, initialPitch);

            lerpedHeight = characterActor.BodySize.y;
        }


        private void Update()
        {
            // Check if game is paused
            if (Time.timeScale == 0)
                return; // Stop all camera movement while paused

            if (targetTransform == null)
            {
                enabled = false;
                return;
            }

            var cameraAxes = inputHandlerSettings.InputHandler.GetVector2(axes);

            if (updatePitch)
                deltaPitch = -cameraAxes.y;

            if (updateYaw)
                deltaYaw = cameraAxes.x;

            if (updateZoom)
                deltaZoom = -inputHandlerSettings.InputHandler.GetFloat(zoomAxis);

            var dt = Time.fixedDeltaTime;

            UpdateCamera(dt);
        }

        private void OnEnable()
        {
            this.MMEventStartListening();

            if (characterActor == null)
                return;

            characterActor.OnTeleport += OnTeleport;
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
            if (characterActor == null)
                return;

            characterActor.OnTeleport -= OnTeleport;
        }


        private void OnValidate()
        {
            initialPitch = Mathf.Clamp(initialPitch, -minPitchAngle, maxPitchAngle);
        }

        public void OnMMEvent(UIEvent eventType)
        {
            if (eventType.EventType == UIEventType.OpenVendorConsole ||
                eventType.EventType == UIEventType.OpenFuelConsole) EnableCameraControl(false);
            else if (eventType.EventType == UIEventType.CloseVendorConsole ||
                     eventType.EventType == UIEventType.CloseFuelConsole) EnableCameraControl(true);
        }


        public void ToggleCameraMode()
        {
            cameraMode = cameraMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
        }

        public bool Initialize(Transform targetTransform)
        {
            if (targetTransform == null)
                return false;

            characterActor = targetTransform.GetComponentInBranch<CharacterActor>();

            if (characterActor == null || !characterActor.isActiveAndEnabled)
            {
                UnityEngine.Debug.Log("The character actor component is null, or it is not active/enabled.");
                return false;
            }

            characterRigidbody = characterActor.GetComponent<Rigidbody>();

            inputHandlerSettings.Initialize(gameObject);

            var referenceObject = new GameObject("Camera reference");
            viewReference = referenceObject.transform;

            if (bodyObject != null)
                bodyRenderers = bodyObject.GetComponentsInChildren<Renderer>();

            return true;
        }


        private void OnTeleport(Vector3 position, Quaternion rotation)
        {
            viewReference.rotation = rotation;
            transform.rotation = viewReference.rotation;

            lerpedCharacterUp = characterActor.Up;
            previousLerpedCharacterUp = lerpedCharacterUp;
        }

        private void HandleBodyVisibility()
        {
            if (cameraMode == CameraMode.FirstPerson)
            {
                if (bodyRenderers != null)
                    for (var i = 0; i < bodyRenderers.Length; i++)
                        if (bodyRenderers[i].GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
                        {
                            var skinnedMeshRenderer = (SkinnedMeshRenderer)bodyRenderers[i];
                            if (skinnedMeshRenderer != null)
                                skinnedMeshRenderer.forceRenderingOff = hideBody;
                        }
                        else
                        {
                            bodyRenderers[i].enabled = !hideBody;
                        }
            }
            else
            {
                if (bodyRenderers != null)
                    for (var i = 0; i < bodyRenderers.Length; i++)
                    {
                        if (bodyRenderers[i] == null)
                            continue;

                        if (bodyRenderers[i].GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
                        {
                            var skinnedMeshRenderer = (SkinnedMeshRenderer)bodyRenderers[i];
                            if (skinnedMeshRenderer != null)
                                skinnedMeshRenderer.forceRenderingOff = false;
                        }
                        else
                        {
                            bodyRenderers[i].enabled = true;
                        }
                    }
            }
        }


        private void UpdateCamera(float dt)
        {
            // Body visibility ---------------------------------------------------------------------
            HandleBodyVisibility();

            // Rotation -----------------------------------------------------------------------------------------
            lerpedCharacterUp = targetTransform.up;

            // Rotate the reference based on the lerped character up vector 
            var deltaRotation = Quaternion.FromToRotation(previousLerpedCharacterUp, lerpedCharacterUp);
            previousLerpedCharacterUp = lerpedCharacterUp;

            viewReference.rotation = deltaRotation * viewReference.rotation;


            // Yaw rotation -----------------------------------------------------------------------------------------        
            viewReference.Rotate(lerpedCharacterUp, deltaYaw * yawSpeed * dt, Space.World);

            // Pitch rotation -----------------------------------------------------------------------------------------            

            var angleToUp = Vector3.Angle(viewReference.forward, lerpedCharacterUp);


            var minPitch = -angleToUp + (90f - minPitchAngle);
            var maxPitch = 180f - angleToUp - (90f - maxPitchAngle);

            var pitchAngle = Mathf.Clamp(deltaPitch * pitchSpeed * dt, minPitch, maxPitch);
            viewReference.Rotate(Vector3.right, pitchAngle);

            // Roll rotation -----------------------------------------------------------------------------------------    
            if (updateRoll)
                viewReference.up =
                    lerpedCharacterUp; //Quaternion.FromToRotation( viewReference.up , lerpedCharacterUp ) * viewReference.up;

            // Position of the target -----------------------------------------------------------------------
            characterPosition = targetTransform.position;

            lerpedHeight = Mathf.Lerp(lerpedHeight, characterActor.BodySize.y, heightLerpSpeed * dt);
            var targetPosition = characterPosition + targetTransform.up * lerpedHeight +
                                 targetTransform.TransformDirection(offsetFromHead);
            viewReference.position = targetPosition;

            var finalPosition = viewReference.position;

            // ------------------------------------------------------------------------------------------------------
            if (cameraMode == CameraMode.ThirdPerson)
            {
                currentDistanceToTarget += deltaZoom * zoomInOutSpeed * dt;
                currentDistanceToTarget = Mathf.Clamp(currentDistanceToTarget, minZoom, maxZoom);

                smoothedDistanceToTarget = Mathf.Lerp(smoothedDistanceToTarget, currentDistanceToTarget,
                    zoomInOutLerpSpeed * dt);
                var displacement = -viewReference.forward * smoothedDistanceToTarget;

                if (collisionDetection)
                {
                    var hit = DetectCollisions(ref displacement, targetPosition);

                    if (collisionAffectsZoom && hit)
                        currentDistanceToTarget = smoothedDistanceToTarget = displacement.magnitude;
                }

                finalPosition = targetPosition + displacement;
            }


            transform.position = finalPosition;
            transform.rotation = viewReference.rotation;
        }


        private bool DetectCollisions(ref Vector3 displacement, Vector3 lookAtPosition)
        {
            var hits = Physics.SphereCastNonAlloc(
                lookAtPosition,
                detectionRadius,
                Vector3.Normalize(displacement),
                hitsBuffer,
                currentDistanceToTarget,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            // Order the results
            var validHitsNumber = 0;
            for (var i = 0; i < hits; i++)
            {
                var hitBuffer = hitsBuffer[i];

                var detectedRigidbody = hitBuffer.collider.attachedRigidbody;

                // Filter the results ---------------------------
                if (hitBuffer.distance == 0)
                    continue;

                if (detectedRigidbody != null)
                {
                    if (considerKinematicRigidbodies && !detectedRigidbody.isKinematic)
                        continue;

                    if (considerDynamicRigidbodies && detectedRigidbody.isKinematic)
                        continue;

                    if (detectedRigidbody == characterRigidbody)
                        continue;
                }

                //----------------------------------------------            
                validHits[validHitsNumber] = hitBuffer;
                validHitsNumber++;
            }

            if (validHitsNumber == 0)
                return false;


            var distance = Mathf.Infinity;
            for (var i = 0; i < validHitsNumber; i++)
            {
                var hitBuffer = validHits[i];

                if (hitBuffer.distance < distance)
                    distance = hitBuffer.distance;
            }

            displacement = CustomUtilities.Multiply(Vector3.Normalize(displacement), distance);


            return true;
        }

        public void EnableCameraControl(bool enable)
        {
            updateYaw = enable;
            updatePitch = enable;
            updateZoom = enable;

            if (!enable)
            {
                deltaYaw = 0f;
                deltaPitch = 0f;
                deltaZoom = 0f;
            }
        }
    }
}