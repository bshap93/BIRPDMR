using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Domains.Gameplay.Equipment.Scripts
{
    public class PickaxeAnimationHelper : MonoBehaviour
    {
        [SerializeField] private DOTweenAnimation pickaxeElasticLocalMove;


        public void OnPickaxeUse()
        {
            if (pickaxeElasticLocalMove != null) StartCoroutine(PickaxeAnimation());
        }

        private IEnumerator PickaxeAnimation()
        {
            if (pickaxeElasticLocalMove != null)
            {
                pickaxeElasticLocalMove.DOPlay();
                yield return new WaitForSeconds(pickaxeElasticLocalMove.duration);
                pickaxeElasticLocalMove.DORestart(); // Resets and plays from beginning
            }
        }
    }
}