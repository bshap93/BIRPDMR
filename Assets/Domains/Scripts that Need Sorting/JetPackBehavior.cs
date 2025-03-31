using Domains.Player.Events;
using Domains.Player.Scripts;
using UnityEngine;

namespace Domains.Scripts_that_Need_Sorting
{
    public class JetPackBehavior : MonoBehaviour
    {
        public void JetPackBehaviorMethod()
        {
            FuelEvent.Trigger(FuelEventType.ConsumeFuel, 5f, PlayerFuelManager.MaxFuelPoints);
        }
    }
}