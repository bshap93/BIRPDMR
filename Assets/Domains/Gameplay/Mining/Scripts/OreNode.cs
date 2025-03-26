using System.Collections;
using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Mining.Scripts

{
    public class OreNode : MonoBehaviour
    {
        [SerializeField] private GameObject pieces;
        [SerializeField] private GameObject refinedPickup;
        [SerializeField] private int dropOnHit;
        [SerializeField] private int hitsToDestroy;
        [SerializeField] private int dropOnDestroy;
        [SerializeField] private Vector3 knockAngle;
        [SerializeField] private AnimationCurve knockCurve;
        [SerializeField] private float knockDuration = 1;

        [SerializeField] private MMFeedbacks oreHitFeedback;
        [SerializeField] private MMFeedbacks oreDestroyFeedback;
        public MMFeedbacks OreHitBehavior;
        private int dropIndex;
        private int hitIndex;


        // On click trigger.
        private void OnMouseDown()
        {
            if (!PlayerStaminaManager.IsPlayerOutOfStamina())
                OreHitBehavior?.PlayFeedbacks();
        }

        // Sets number of pickups to spawn.
        public void oreHit()
        {
            StaminaEvent.Trigger(StaminaEventType.ConsumeStamina, 2f);
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

            for (var i = 0; i < dropIndex; i++)
            {
                var randomPosition = new Vector3(Random.Range(minX, maxX), centerY, Random.Range(minZ, maxZ));

                Instantiate(refinedPickup, randomPosition, Quaternion.Euler(0, Random.Range(0, 360), 0));
            }

            if (hitIndex < hitsToDestroy) //Controls when to shatter.
            {
                // Knock animation.
                StartCoroutine(Animate());
                oreHitFeedback?.PlayFeedbacks();
            }
            else
            {
                //Spawn pieces and destroy.
                oreDestroyFeedback?.PlayFeedbacks();
                var position = transform.position;
                var rotation = transform.rotation;
                Instantiate(pieces, position, rotation);
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