using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Gameplay.Equipment.Scripts
{
    public class PickaxeSpringController : MonoBehaviour
    {
        [SerializeField] private MMSpringPosition springPosition;
        [SerializeField] private Vector3 moveToValue;

        [FormerlySerializedAs("hammerAnimationDelay")] [SerializeField]
        private float pickaxeAnimationDelay;

        public void OnHammerUse()
        {
            if (springPosition != null) StartCoroutine(SpringHammer());
        }

        private IEnumerator SpringHammer()
        {
            springPosition.MoveToSubtractive(moveToValue);
            yield return new WaitForSeconds(pickaxeAnimationDelay);
            springPosition.MoveToAdditive(moveToValue);
        }
    }
}