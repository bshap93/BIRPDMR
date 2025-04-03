using System.Collections;
using Domains.Items;
using Domains.Player.Events;
using Domains.Player.Scripts;
using Domains.Scene.Scripts;
using Gameplay.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Mining.Scripts

{
    public class OreNode : MonoBehaviour
    {
        [SerializeField] private GameObject pieces;
        [SerializeField] private BaseItem itemTypeMined;
        [SerializeField] private int dropOnHit;
        [SerializeField] private int hitsToDestroy;
        [SerializeField] private int dropOnDestroy;
        [SerializeField] private Vector3 knockAngle;
        [SerializeField] private AnimationCurve knockCurve;
        [SerializeField] private float knockDuration = 1;

        [SerializeField] private MMFeedbacks oreHitFeedback;
        [SerializeField] private MMFeedbacks oreDestroyFeedback;
        public MMFeedbacks OreHitBehavior;

        // Unique ID for the ore node.
        public string UniqueID;
        private int dropIndex;
        private int hitIndex;

        private void Start()
        {
            StartCoroutine(InitializeAfterDestructableManager());
        }


        // On click trigger.
        private void OnMouseDown()
        {
            if (!PlayerFuelManager.IsPlayerOutOfFuel())
                OreHitBehavior?.PlayFeedbacks();
        }

        private IEnumerator InitializeAfterDestructableManager()
        {
            // Wait a frame to ensure PickableManager has initialized
            yield return null;

            // Now check if this item should be destroyed
            if (DestructableManager.IsDestuctableDestroyed(UniqueID)) Destroy(gameObject);
        }

        // Sets number of pickups to spawn.
        public void oreHit()
        {
            var currentFuel = PlayerFuelManager.FuelPoints;
            var maxFuel = PlayerFuelManager.MaxFuelPoints;
            FuelEvent.Trigger(FuelEventType.ConsumeFuel, 2f, maxFuel);
            hitIndex++;
            if (hitIndex < hitsToDestroy)
                dropIndex = dropOnHit;
            else
                dropIndex = dropOnDestroy;

            // Gets node bounds for pickup spawn location.
            var renderer = GetComponent<Renderer>();
            var worldBounds = renderer.bounds;
            var minX = worldBounds.min.x;
            var maxX = worldBounds.max.x;
            var centerY = worldBounds.center.y;
            var minZ = worldBounds.min.z;
            var maxZ = worldBounds.max.z;

            var inventoryManager = PlayerInventoryManager.Instance;
            if (inventoryManager != null)
                for (var i = 0; i < dropIndex; i++)
                {
                    var entry = new Inventory.InventoryEntry(UniqueID, itemTypeMined);
                    if (inventoryManager.AddItem(entry)) ItemEvent.Trigger(ItemEventType.Picked, entry, transform);
                    else
                        UnityEngine.Debug.LogWarning("Inventory full! Cannot pick up item.");
                }

            if (hitIndex < hitsToDestroy) //Controls when to shatter.
            {
                // Knock animation.
                StartCoroutine(Animate());
                oreHitFeedback?.PlayFeedbacks();
            }
            else
            {
                // Spawn pieces and destroy.
                oreDestroyFeedback?.PlayFeedbacks();
                var position = transform.position;
                var rotation = transform.rotation;
                var spawnedPieces = Instantiate(pieces, position, rotation);

                DestructableEvent.Trigger(DestructableEventType.Destroyed, UniqueID);
                Destroy(gameObject);
            }
        }


        private IEnumerator Animate() //Knock animation coroutine.
        {
            float t = 0;
            while (t < knockDuration)
            {
                var v = knockCurve.Evaluate(t / knockDuration);
                transform.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(knockAngle), v);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}