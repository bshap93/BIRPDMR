using Domains.Player.Events;
using UnityEngine;

public class BedObject : MonoBehaviour
{
    public void TriggerRestoreStamina()
    {
        StaminaEvent.Trigger(StaminaEventType.FullyRecoverStamina, 100);
        Debug.Log("Restoring stamina");
    }
}
