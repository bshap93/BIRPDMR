using Domains.Player.Events;
using UnityEngine;

public class BedObject : MonoBehaviour
{
    public void TriggerRestoreStamina()
    {
        FuelEvent.Trigger(FuelEventType.FullyRecoverStamina, 100);
        Debug.Log("Restoring stamina");
    }
}