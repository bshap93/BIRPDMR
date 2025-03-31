using Domains.Player.Events;
using UnityEngine;

namespace Domains.Scripts_that_Need_Sorting
{
    public class JetPackBehavior : MonoBehaviour
    {
        public void JetPackBehaviorMethod()
        {
            FuelEvent.Trigger(FuelEventType.ConsumeStamina, 5f);
        }
    }
}