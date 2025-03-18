using Domains.UI_Global.Events;
using UnityEngine;

public class TriggerCloseUpgradeUI : MonoBehaviour
{
    public void TriggerCloseUI()
    {
        Debug.Log("TriggerCloseUI");
        UIEvent.Trigger(UIEventType.CloseVendorConsole);
    }
}