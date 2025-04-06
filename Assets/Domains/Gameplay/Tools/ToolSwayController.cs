using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

namespace Domains.Gameplay.Tools
{
    public class ToolSwayController : MonoBehaviour
    {
        public float swayAmount = 0.1f;
        public float swaySpeed = 4f;
        public CharacterActor character;

        private Vector3 initialLocalPosition;

        private void Start()
        {
            initialLocalPosition = transform.localPosition;
            if (character == null)
                character = FindFirstObjectByType<CharacterActor>();
        }

        private void Update()
        {
            if (character == null) return;

            var velocity = character.PlanarVelocity.magnitude;
            if (velocity > 0.1f)
            {
                float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
                transform.localPosition = initialLocalPosition + new Vector3(0, sway, 0);
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * 4f);
            }
        }
    }

}