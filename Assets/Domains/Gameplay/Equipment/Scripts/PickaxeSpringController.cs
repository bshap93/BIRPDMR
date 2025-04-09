using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Equipment.Scripts
{
    public class PickaxeSpringController : MonoBehaviour
    {
        [SerializeField] private MMSpringPosition springPosition;
        [SerializeField] private Vector3 moveToValue;
        [SerializeField] private float hammerAnimationDelay;

        public void OnHammerUse()
        {
            if (springPosition != null) StartCoroutine(SpringHammer());
        }

        private IEnumerator SpringHammer()
        {
            springPosition.MoveToSubtractive(moveToValue);
            yield return new WaitForSeconds(hammerAnimationDelay);
            springPosition.MoveToAdditive(moveToValue);
        }
    }
}