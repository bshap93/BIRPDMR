using System;
using Domains.Gameplay.Mining.Scripts;
using Domains.Mining.Scripts;
using Domains.Player.Scripts;
using Gameplay.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;
using Domains.Input.Scripts; // Added for CustomInputBindings

namespace Domains.Items
{
    public class ItemPicker : MonoBehaviour, IInteractable
    {
        [FormerlySerializedAs("UniqueID")] public string uniqueID;
        [FormerlySerializedAs("ItemType")] public BaseItem itemType;
        [FormerlySerializedAs("Quantity")] public int quantity = 1;

        [Header("Interaction Settings")] [Tooltip("How long the interact key must be held to pick up the item")]
        public float interactionHoldTime = 1.0f; // Time required to hold the interact key

        [Header("Feedbacks")] [Tooltip("Feedbacks to play when the item is picked up")]
        public MMFeedbacks pickedMmFeedbacks; // Feedbacks to play when the item is picked up

        [Tooltip("Feedbacks to play when the item is sold")]
        public MMFeedbacks soldMmFeedbacks; // Feedbacks to play when the item is sold

        [FormerlySerializedAs("NotPickable")] public bool notPickable; // If true, the item cannot be picked up
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private bool _isBeingDestroyed;
#pragma warning restore CS0414 // Field is assigned but its value is never used

#pragma warning disable CS0414 // Field is assigned but its value is never used
        private bool _isInRange;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        private Inventory _targetInventory;
        public GameObject interactablePrompt;

        // Track interaction state
        private bool _isInteracting = false;
        private float _interactionTimer = 0f;
        private bool _interactionComplete = false;

        private void Awake()
        {
            if (string.IsNullOrEmpty(uniqueID))
            {
                uniqueID = Guid.NewGuid().ToString(); // Generate only if unset
                UnityEngine.Debug.LogWarning($"Generated new UniqueID for {gameObject.name}: {uniqueID}");
            }
        }

        private void Start()
        {
            if (PickableManager.IsItemPicked(uniqueID))
            {
                Destroy(gameObject);
                return;
            }

            _targetInventory = FindFirstObjectByType<Inventory>();

            if (_targetInventory == null) UnityEngine.Debug.LogWarning("No inventory found in scene");

            if (pickedMmFeedbacks != null) pickedMmFeedbacks.Initialization(gameObject);
        }

        private void Update()
        {
            // Track interaction progress only when interacting
            if (_isInteracting)
            {
                // If interaction key is still held
                if (CustomInputBindings.IsInteractPressed())
                {
                    _interactionTimer += Time.deltaTime;

                    UnityEngine.Debug.Log("Interaction Timer: " + _interactionTimer);

                    // Check if we've reached the required hold time
                    if (_interactionTimer >= interactionHoldTime && !_interactionComplete)
                    {
                        PickItem();
                        _interactionComplete = true;
                    }
                }
                else
                {
                    // Reset if key released before completion
                    ResetInteraction();
                }
            }
        }

        private void OnDestroy()
        {
            _isBeingDestroyed = true;

            _isInRange = false;
            enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) _isInRange = true;
        }

        private void OnTriggerExit(Collider exitCollider)
        {
            if (exitCollider.CompareTag("Player")) _isInRange = false;
        }

        public void Interact()
        {
            UnityEngine.Debug.Log("Interacting with item");
            // Start the interaction timer instead of immediately picking
            if (!_isInteracting && !_interactionComplete)
            {
                _isInteracting = true;
                _interactionTimer = 0f;
            }
        }

        private void ResetInteraction()
        {
            _isInteracting = false;
            _interactionTimer = 0f;
        }

        public void ShowInteractablePrompt()
        {
            if (interactablePrompt != null) interactablePrompt.SetActive(true);
        }

        public void HideInteractablePrompt()
        {
            if (interactablePrompt != null) interactablePrompt.SetActive(false);
        }

        public void PickItem()
        {
            if (_targetInventory != null)
            {
                // Only add if not already in inventory
                if (_targetInventory.GetItem(uniqueID) != null)
                {
                    UnityEngine.Debug.LogWarning("Item already in inventory! Skipping pickup.");
                    return;
                }

                var entry = new Inventory.InventoryEntry(uniqueID, itemType);
                if (_targetInventory.AddItem(entry))
                {
                    // Play feedback
                    pickedMmFeedbacks?.PlayFeedbacks();

                    // Save item as picked
                    PickableManager.SavePickedItem(uniqueID, true);
                    PickableManager.PickedItems.Add(uniqueID);

                    // Trigger event
                    ItemEvent.Trigger(ItemEventType.Picked, entry, transform);

                    // Destroy
                    Destroy(gameObject);
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Inventory full! Cannot pick up item.");
                }
            }
        }
    }
}