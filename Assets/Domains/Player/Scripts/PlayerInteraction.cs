using System;
using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Input.Scripts;
using Domains.Scripts_that_Need_Sorting;
using UnityEngine;
using UnityEngine.Serialization;
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

        public LayerMask playerLayerMask;

        public bool[] diggableLayers;

        public float currentDigDepth;


        [FormerlySerializedAs("forwardTextureDetector")] [FormerlySerializedAs("ForwardTextureDetector")]
        public TerrainLayerDetector forwardTerrainLayerDetector;

        private DiggerMasterRuntime _diggerMasterRuntime;
        private bool _interactablePrompt;

        private void Start()
        {
            FindFirstObjectByType<DiggerMaster>();
            _diggerMasterRuntime = FindFirstObjectByType<DiggerMasterRuntime>();
            GetComponent<RuntimeDig>();

            // Find the TextureDetector in the scene

            if (forwardTerrainLayerDetector == null)
                UnityEngine.Debug.LogWarning(
                    "TextureDetector not found in the scene. Cannot track texture information.");
        }

        private void Update()
        {
            PerformRaycastCheck(); // ✅ Single raycast for both interactables and diggable terrain

            // Update texture information from TextureDetector if available
            UpdateTextureInformation();
            currentDigDepth = transform.position.y;

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



        // New method to update texture information
        private void UpdateTextureInformation()
        {
            if (forwardTerrainLayerDetector != null && !string.IsNullOrEmpty(forwardTerrainLayerDetector.texture))
                // Extract name and index from TextureDetector's texture string
                if (ExtractNameAndIndex(forwardTerrainLayerDetector.texture, out var name, out var index))
                {
                    // Update our tracking variables
                }
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

            var terrMask = terrainLayer & ~playerLayerMask;

            // First check if there's terrain blocking the view
            RaycastHit terrainHit;
            var terrainBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, interactionDistance, terrMask);

            var interactMask = interactableLayer & ~playerLayerMask;

            // Then check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, interactionDistance, interactMask);

            // If we hit both, check if the terrain is in front of the interactable
            if (terrainBlocking && hitInteractable)
                // If terrain is closer than the interactable, it's blocking
                if (terrainHit.distance < interactableHit.distance)
                {
                    // Terrain is blocking, reset reticle and hide prompts
                    reticle.color = defaultReticleColor;
                    if (_interactablePrompt)
                        _interactablePrompt = false;
                    HideAllPrompts();
                    return;
                }

            // If we reach here, either there's no terrain blocking, or the interactable is in front of terrain
            if (hitInteractable)
            {
                var interactable = interactableHit.collider.GetComponent<IInteractable>();
                var button = interactableHit.collider.GetComponent<ButtonActivated>();

                if (interactable != null)
                {
                    reticle.color = interactReticleColor;
                    interactable.ShowInteractablePrompt();
                    _interactablePrompt = true;

                    // Show button prompt if applicable
                    if (button != null) button.ShowInteractablePrompt();
                    return;
                }
            }

            // Reset if no interactable is found or if it's blocked
            reticle.color = defaultReticleColor;
            if (_interactablePrompt)
                _interactablePrompt = false;

            HideAllPrompts(); // Hide button prompts if nothing is targeted
        }

        private void HideAllPrompts()
        {
            foreach (var button in FindObjectsByType<ButtonActivated>(FindObjectsSortMode.None))
                button.HideInteractablePrompt();
        }

        /// <summary>
        ///     Extracts name and index from a string in the format "name: Topsoil | index: 0"
        /// </summary>
        /// <param name="input">The formatted string to parse</param>
        /// <param name="name">Output parameter that will contain the extracted name</param>
        /// <param name="index">Output parameter that will contain the extracted index</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        public bool ExtractNameAndIndex(string input, out string name, out int index)
        {
            // Initialize output parameters with default values
            name = string.Empty;
            index = -1;

            // Check if input is null or empty
            if (string.IsNullOrEmpty(input))
                return false;

            try
            {
                // Split the input by the separator '|'
                var parts = input.Split('|');

                if (parts.Length < 2)
                    return false;

                // Extract name part and trim whitespace
                var namePart = parts[0].Trim();

                // Extract index part and trim whitespace
                var indexPart = parts[1].Trim();

                // Check if the parts start with expected prefixes
                if (!namePart.StartsWith("name:") || !indexPart.StartsWith("index:"))
                    return false;

                // Extract the actual name (remove "name: " prefix and trim)
                name = namePart.Substring(5).Trim();

                // Extract the actual index (remove "index: " prefix and trim)
                var indexValue = indexPart.Substring(6).Trim();

                // Parse index to integer
                if (!int.TryParse(indexValue, out index))
                    return false;

                return true;
            }
            catch (Exception)
            {
                // Return false in case of any exception
                return false;
            }
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

            var interactMask = interactableLayer & ~playerLayerMask;
            var terrMask = terrainLayer & ~playerLayerMask;

            // First check if terrain is blocking
            RaycastHit terrainHit;
            var terrainBlocking = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, interactionDistance, terrMask);

            // Then check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, interactionDistance, interactMask);

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