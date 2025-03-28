using Domains.Player.Events;
using UnityEngine;

namespace Domains.Scripts_that_Need_Sorting
{
    public class JetPackBehavior : MonoBehaviour
    {
        public void JetPackBehaviorMethod()
        {
            StaminaEvent.Trigger(StaminaEventType.ConsumeStamina, 5f);
        }
    }
}