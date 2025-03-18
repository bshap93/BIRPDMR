using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Scene.Scripts
{
    public class DpdSceneManager : MonoBehaviour
    {
        [Header("Character")] [SerializeField] private CharacterActor characterActor;

        [Header("Scene references")] [SerializeField]
        private CharacterReferenceObject[] references;


        [SerializeField] private bool hideAndConfineCursor = true;

        [Header("Graphics")] [SerializeField] private GameObject graphicsObject;

        [FormerlySerializedAs("camera")] [Header("Camera")] [SerializeField]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        private Camera3D myCamera;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        [FormerlySerializedAs("frameRateText")] [SerializeField]

        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        private Renderer[] graphicsRenderers;

        private readonly Renderer[] capsuleRenderers = null;

        private NormalMovement normalMovement;

        private void Awake()
        {
            if (characterActor != null)
                normalMovement = characterActor.GetComponentInChildren<NormalMovement>();

            // Set the looking direction mode
            if (normalMovement != null && myCamera != null)
            {
                if (myCamera.cameraMode == Camera3D.CameraMode.FirstPerson)
                    normalMovement.lookingDirectionParameters.lookingDirectionMode =
                        LookingDirectionParameters.LookingDirectionMode.ExternalReference;
                else
                    normalMovement.lookingDirectionParameters.lookingDirectionMode =
                        LookingDirectionParameters.LookingDirectionMode.Movement;
            }

            

            if (graphicsObject != null)
                graphicsRenderers = graphicsObject.GetComponentsInChildren<Renderer>(true);

            Cursor.visible = !hideAndConfineCursor;
            Cursor.lockState = hideAndConfineCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }


        private float GetRefreshRateValue()
        {
#if UNITY_2022_2_OR_NEWER
            return (float)Screen.currentResolution.refreshRateRatio.value;
#else
            return Screen.currentResolution.refreshRate;
#endif
        }


        private void HandleVisualObjects(bool showCapsule)
        {
            if (capsuleRenderers != null)
                for (var i = 0; i < capsuleRenderers.Length; i++)
                    capsuleRenderers[i].enabled = showCapsule;

            if (graphicsRenderers != null)
                for (var i = 0; i < graphicsRenderers.Length; i++)
                {
                    var skinnedMeshRenderer = (SkinnedMeshRenderer)graphicsRenderers[i];
                    if (skinnedMeshRenderer != null)
                        skinnedMeshRenderer.forceRenderingOff = showCapsule;
                    else
                        graphicsRenderers[i].enabled = !showCapsule;
                }
        }

        private void GoTo(CharacterReferenceObject reference)
        {
            if (reference == null)
                return;

            if (characterActor == null)
                return;

            characterActor.constraintUpDirection = reference.referenceTransform.up;
            characterActor.Teleport(reference.referenceTransform);

            characterActor.upDirectionReference = reference.verticalAlignmentReference;
            characterActor.upDirectionReferenceMode = VerticalAlignmentSettings.VerticalReferenceMode.Away;
        }
    }
}