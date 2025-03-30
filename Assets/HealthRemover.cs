using Domains.Player.Events;
using UnityEngine;

public class HealthRemover : MonoBehaviour
{
    public void HurtPlayer(float damage = 10)
    {
        HealthEvent.Trigger(HealthEventType.ConsumeHealth, damage);
    }
}