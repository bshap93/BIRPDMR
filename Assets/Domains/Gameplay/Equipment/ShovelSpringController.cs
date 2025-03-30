using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Equipment
{
    public class ShovelSpringController : MonoBehaviour
    {
        [SerializeField] private MMSpringPosition springPosition;
        [SerializeField] private Vector3 moveToValue;
        [SerializeField] private float shovelAnimationDelay;

        public void OnShovelUse()
        {
            if (springPosition != null) StartCoroutine(SpringShovel());
        }

        private IEnumerator SpringShovel()
        {
            springPosition.MoveToSubtractive(moveToValue);
            yield return new WaitForSeconds(shovelAnimationDelay);
            springPosition.MoveToAdditive(moveToValue);
        }
    }
}