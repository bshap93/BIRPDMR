using Domains.UI_Global.Events;
using UnityEngine;

namespace Domains.UI_Global.Triggers
{
    public class TriggerForCloseUI : MonoBehaviour
    {
        public void TriggerCloseVendorUI()
        {
            UnityEngine.Debug.Log("TriggerCloseUI");
            UIEvent.Trigger(UIEventType.CloseVendorConsole);
            UIEvent.Trigger(UIEventType.CloseFuelConsole);
        }

        public void TriggerCloseUI()
        {
            UnityEngine.Debug.Log("TriggerCloseUI");
            UIEvent.Trigger(UIEventType.CloseUI);
        }
    }
}