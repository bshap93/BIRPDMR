using System;
using Domains.Mining.Scripts;
using Domains.Player.Scripts;
using Gameplay.Events;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Items
{
    public class ItemPicker : MonoBehaviour, IInteractable
    {
        [FormerlySerializedAs("UniqueID")] public string uniqueID;
        [FormerlySerializedAs("ItemType")] public BaseItem itemType;
        [FormerlySerializedAs("Quantity")] public int quantity = 1;

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
            PickItem();
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