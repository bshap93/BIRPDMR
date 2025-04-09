using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Mining.Scripts
{
    public class DecimatedStonePieces : MonoBehaviour
    {
        public float disintegrationDelay = 1.5f;
        [SerializeField] private MMFeedbacks disintegrationFeedback;
        [SerializeField] private GameObject disintegrationParticles;
        [SerializeField] private GameObject[] pieces;

        private void Awake()
        {
        }

        private IEnumerator Disintegrate()
        {
            yield return new WaitForSeconds(disintegrationDelay);
            Destroy(gameObject);
        }
    }
}