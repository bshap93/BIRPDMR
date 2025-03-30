using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Equipment
{
    public class HammerSpringController : MonoBehaviour
    {
        [SerializeField] private MMSpringPosition springPosition;
        [SerializeField] private MMSpringRotation springRotation;
        [SerializeField] private Vector3 moveToValue;
        [SerializeField] private Vector3 rotateToValue;
        [SerializeField] private float hammerAnimationDelay;

        public void OnHammerUse()
        {
            if (springPosition != null) StartCoroutine(SpringHammer());
        }

        private IEnumerator SpringHammer()
        {
            springPosition.MoveToSubtractive(moveToValue);
            springRotation.MoveToAdditive(rotateToValue);
            yield return new WaitForSeconds(hammerAnimationDelay);
            springPosition.MoveToAdditive(moveToValue);
            springRotation.MoveToSubtractive(rotateToValue);
        }
    }
}