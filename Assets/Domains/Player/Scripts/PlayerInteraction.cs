using Digger;
using Digger.Demo;
using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Input.Scripts;
using Domains.Player.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Domains.Player.Scripts
{
    public class PlayerInteraction : MonoBehaviour
    {
        public float interactionDistance = 2f; // How far the player can interact
        public LayerMask interactableLayer; // Only detect objects in this layer
        public LayerMask terrainLayer; // Only detect objects in this layer
        public Camera playerCamera; // Reference to the player’s camera
        public Image reticle;
        public Color defaultReticleColor = Color.white;
        public Color interactReticleColor = Color.green;

        private RuntimeDig _digClass;
        private DiggerMaster _diggerMaster;
        private DiggerMasterRuntime _diggerMasterRuntime;
        private bool _interactablePrompt;

        private string _currentPointedName;


        private void Start()
        {
            _diggerMaster = FindFirstObjectByType<DiggerMaster>();
            _diggerMasterRuntime = FindFirstObjectByType<DiggerMasterRuntime>();
            _digClass = GetComponent<RuntimeDig>();
        }

        private void Update()
        {
            PerformRaycastCheck(); // ✅ Single raycast for both interactables and diggable terrain


            if (CustomInputBindings.IsInteractPressed()) // Press E to interact
                PerformInteraction();

            if (CustomInputBindings.IsPersistanceKeyPressed())
                _diggerMasterRuntime.PersistAll();
            else if (CustomInputBindings.IsDeletionKeyPressed()) _diggerMasterRuntime.DeleteAllPersistedData();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(
                playerCamera.transform.position,
                playerCamera.transform.TransformDirection(Vector3.forward) * interactionDistance);
        }


        private void PerformRaycastCheck()
        {
            if (playerCamera == null)
            {
                UnityEngine.Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            RaycastHit terrainHit;
            var terrainBlocking =
                Physics.Raycast(rayOrigin, rayDirection, out terrainHit, interactionDistance, terrainLayer);

            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(rayOrigin, rayDirection, out interactableHit, interactionDistance,
                interactableLayer);

            if (terrainBlocking && hitInteractable && terrainHit.distance < interactableHit.distance)
            {
                // Terrain is blocking, show terrain texture name
                reticle.color = defaultReticleColor;
                if (_interactablePrompt)
                    _interactablePrompt = false;

                HideAllPrompts();

                // ✅ Detect texture and send name to UI
                UnityEngine.Debug.Log("Detecting texture...");
                var textureName = DetectTexture(terrainHit);
                UpdatePointedName(textureName);
                return;
            }

            if (hitInteractable)
            {
                var interactable = interactableHit.collider.GetComponent<IInteractable>();
                var button = interactableHit.collider.GetComponent<ButtonActivated>();

                if (interactable != null)
                {
                    reticle.color = interactReticleColor;
                    interactable.ShowInteractablePrompt();
                    _interactablePrompt = true;

                    if (button != null)
                        button.ShowInteractablePrompt();

                    // ✅ Set name for UI
                    UpdatePointedName(interactableHit.collider.name);
                    return;
                }
            }

            // Nothing hit
            reticle.color = defaultReticleColor;
            if (_interactablePrompt)
                _interactablePrompt = false;

            HideAllPrompts();
            UpdatePointedName(""); // Clear UI
        }

        private void UpdatePointedName(string newName)
        {
            if (_currentPointedName != newName)
            {
                _currentPointedName = newName;
                PointedObjectEvent.Trigger(PointedObjectEventType.PointedObjectChanged, newName);
            }
        }


        private void HideAllPrompts()
        {
            foreach (var button in FindObjectsByType<ButtonActivated>(FindObjectsSortMode.None))
                button.HideInteractablePrompt();
        }


        private string DetectTexture(RaycastHit hit)
        {
            var index = TextureDetector.GetTextureIndex(hit, out var terrain);
            UnityEngine.Debug.Log("Texture index: " + index);
            if (terrain != null && index < terrain.terrainData.terrainLayers.Length)
                return terrain.terrainData.terrainLayers[index].name;

            return "Unknown Terrain";
        }


        private void PerformInteraction()
        {
            if (playerCamera == null)
            {
                UnityEngine.Debug.LogError("PlayerInteraction: No camera assigned!");
                return;
            }

            var rayOrigin = playerCamera.transform.position;
            var rayDirection = playerCamera.transform.forward;

            // First check if terrain is blocking
            RaycastHit terrainHit;
            var terrainBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, interactionDistance, terrainLayer);

            // Then check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, interactionDistance, interactableLayer);

            // Only interact if:
            // 1. We hit an interactable AND
            // 2. Either there's no terrain blocking OR the interactable is closer than the terrain
            if (hitInteractable && (!terrainBlocking || interactableHit.distance < terrainHit.distance))
            {
                var interactable = interactableHit.collider.GetComponent<IInteractable>();
                if (interactable != null) interactable.Interact();
            }
        }
    }
}