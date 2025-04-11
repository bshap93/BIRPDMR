using System.Collections;
using UnityEngine;

namespace Domains.Gameplay.Equipment.Scripts
{
    public class PickaxeAnimationHelper : MonoBehaviour
    {
        public void OnPickaxeUse()
        {
        }

        private IEnumerator PickaxeAnimation()
        {
            yield return new WaitForSeconds(0.1f);
        }
    }
}