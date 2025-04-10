using Domains.Player.Scripts;
using UnityEngine;

namespace Domains.Scripts_that_Need_Sorting
{
    public class SpotlightController : MonoBehaviour
    {
        public Light digSpotlight;
        public float spotlightStrengthenDepth = 2f;

        public float initialSpotlightAngle = 30f;
        public float initialSpotlightIntensity = 1f;

        public float increasedSpotlightAngle = 45f;
        public float increasedSpotlightIntensity = 1.5f;

        private PlayerInteraction playerInteraction;

        private void Awake()
        {
            playerInteraction = GetComponent<PlayerInteraction>();
        }

        private void Update()
        {
            UpdateLight();
        }

        private void UpdateLight()
        {
            if (digSpotlight != null)
            {
                var depth = playerInteraction.currentDigDepth;
                if (Mathf.Abs(depth) > spotlightStrengthenDepth)
                {
                    digSpotlight.spotAngle = increasedSpotlightAngle;
                    digSpotlight.intensity = increasedSpotlightIntensity;
                }
                else
                {
                    digSpotlight.spotAngle = initialSpotlightAngle;
                    digSpotlight.intensity = initialSpotlightIntensity;
                }
            }
        }
    }
}