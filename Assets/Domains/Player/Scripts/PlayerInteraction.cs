using System;
using System.Collections.Generic;
using System.Drawing;
using Digger;
using Digger.Demo;
using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Input.Scripts;
using Domains.Player.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Color = UnityEngine.Color;

[Serializable]
public enum PointedObjectType
{
    Terrain,
    Interactable,
    None
}

namespace Domains.Player.Scripts
{
    // Define a class to hold texture/object information for UI display
    [Serializable]
    public class PointedObjectInfo
    {
        public string name;
        public PointedObjectType type; // "Terrain", "Interactable", etc.
        public int textureIndex = -1;
        public bool isInteractable;
    }

    // Event for sending pointed object information


    public class PlayerInteraction : MonoBehaviour
    {
        public float interactionDistance = 2f;
        public LayerMask interactableLayer;
        public LayerMask terrainLayer;
        public Camera playerCamera;
        public Image reticle;
        public Color defaultReticleColor = Color.white;
        public Color interactReticleColor = Color.green;
        public Color terrainReticleColor = Color.blue; // Added color for when pointing at terrain

        // Event that will be invoked when pointed object changes

        public List<PointedObjectInfo> diggablePointedObjects;


        private RuntimeDig _digClass;
        private DiggerMaster _diggerMaster;
        private DiggerMasterRuntime _diggerMasterRuntime;
        private bool _interactablePrompt;
        private PointedObjectInfo _currentPointedObject = new();

        private void Start()
        {
            _diggerMaster = FindFirstObjectByType<DiggerMaster>();
            _diggerMasterRuntime = FindFirstObjectByType<DiggerMasterRuntime>();
            _digClass = GetComponent<RuntimeDig>();

            // Initialize with empty info
            _currentPointedObject.name = "";
            _currentPointedObject.type = PointedObjectType.None;
            _currentPointedObject.isInteractable = false;
        }

        private void Update()
        {
            PerformRaycastCheck();

            if (CustomInputBindings.IsInteractPressed())
                PerformInteraction();

            if (CustomInputBindings.IsPersistanceKeyPressed())
                _diggerMasterRuntime.PersistAll();
            else if (CustomInputBindings.IsDeletionKeyPressed())
                _diggerMasterRuntime.DeleteAllPersistedData();
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

            // Check for terrain
            RaycastHit terrainHit;
            var terrainHitSuccess = Physics.Raycast(
                rayOrigin, rayDirection, out terrainHit, interactionDistance, terrainLayer);

            // Check for interactables
            RaycastHit interactableHit;
            var hitInteractable = Physics.Raycast(
                rayOrigin, rayDirection, out interactableHit, interactionDistance, interactableLayer);

            // Reset current pointed object
            var objectChanged = false;
            var previousObject = new PointedObjectInfo
            {
                name = _currentPointedObject.name,
                type = _currentPointedObject.type,
                textureIndex = _currentPointedObject.textureIndex,
                isInteractable = _currentPointedObject.isInteractable
            };

            // If we hit both, determine which is closer
            if (terrainHitSuccess && hitInteractable)
            {
                if (terrainHit.distance < interactableHit.distance)
                {
                    // Terrain is closer
                    HandleTerrainHit(terrainHit);
                    objectChanged = !ComparePointedObjects(previousObject, _currentPointedObject);
                }
                else
                {
                    // Interactable is closer
                    HandleInteractableHit(interactableHit);
                    objectChanged = !ComparePointedObjects(previousObject, _currentPointedObject);
                }
            }
            else if (terrainHitSuccess)
            {
                // Only hit terrain
                HandleTerrainHit(terrainHit);
                objectChanged = !ComparePointedObjects(previousObject, _currentPointedObject);
            }
            else if (hitInteractable)
            {
                // Only hit interactable
                HandleInteractableHit(interactableHit);
                objectChanged = !ComparePointedObjects(previousObject, _currentPointedObject);
            }
            else
            {
                // Hit nothing
                reticle.color = defaultReticleColor;
                if (_interactablePrompt)
                {
                    _interactablePrompt = false;
                    HideAllPrompts();
                }

                // Clear current pointed object
                _currentPointedObject.name = "";
                _currentPointedObject.type = PointedObjectType.None;
                _currentPointedObject.textureIndex = -1;
                _currentPointedObject.isInteractable = false;
                objectChanged = !ComparePointedObjects(previousObject, _currentPointedObject);
            }

            // Trigger event if object changed
            if (objectChanged)
                PointedObjectEvent.Trigger(PointedObjectEventType.PointedObjectChanged, _currentPointedObject);
        }

        private void HandleTerrainHit(RaycastHit hit)
        {
            // Update reticle color for terrain
            reticle.color = terrainReticleColor;

            // Hide any interactable prompts
            if (_interactablePrompt)
            {
                _interactablePrompt = false;
                HideAllPrompts();
            }

            // Get terrain texture info
            var textureIndex = TextureDetector.GetTextureIndex(hit, out var terrain);

            // Update current pointed object
            if (terrain != null && textureIndex >= 0 && textureIndex < terrain.terrainData.terrainLayers.Length)
            {
                _currentPointedObject.name = terrain.terrainData.terrainLayers[textureIndex].name;
                _currentPointedObject.type = PointedObjectType.Terrain;
                _currentPointedObject.textureIndex = textureIndex;
                _currentPointedObject.isInteractable = false;
            }
            else
            {
                _currentPointedObject.name = "Unknown Terrain";
                _currentPointedObject.type = PointedObjectType.Terrain;
                _currentPointedObject.textureIndex = -1;
                _currentPointedObject.isInteractable = false;
            }
        }

        private void HandleInteractableHit(RaycastHit hit)
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            var button = hit.collider.GetComponent<ButtonActivated>();

            if (interactable != null)
            {
                reticle.color = interactReticleColor;
                interactable.ShowInteractablePrompt();
                _interactablePrompt = true;

                // Show button prompt if applicable
                if (button != null)
                    button.ShowInteractablePrompt();

                // Update current pointed object
                _currentPointedObject.name = hit.collider.gameObject.name;
                _currentPointedObject.type = PointedObjectType.Interactable;
                _currentPointedObject.textureIndex = -1;
                _currentPointedObject.isInteractable = true;

                return;
            }

            // If we reach here, no interactable was found
            reticle.color = defaultReticleColor;
            if (_interactablePrompt)
            {
                _interactablePrompt = false;
                HideAllPrompts();
            }

            // Clear current pointed object
            _currentPointedObject.name = hit.collider.gameObject.name;
            _currentPointedObject.type = PointedObjectType.Interactable;
            _currentPointedObject.textureIndex = -1;
            _currentPointedObject.isInteractable = false;
        }

        private bool ComparePointedObjects(PointedObjectInfo obj1, PointedObjectInfo obj2)
        {
            return obj1.name == obj2.name &&
                   obj1.type == obj2.type &&
                   obj1.textureIndex == obj2.textureIndex &&
                   obj1.isInteractable == obj2.isInteractable;
        }

        private void HideAllPrompts()
        {
            foreach (var button in FindObjectsByType<ButtonActivated>(FindObjectsSortMode.None))
                button.HideInteractablePrompt();
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