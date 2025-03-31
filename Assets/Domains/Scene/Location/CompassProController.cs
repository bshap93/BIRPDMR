using CompassNavigatorPro;
using Domains.Gameplay.Equipment.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Scene.Location
{
    public class CompassProController : MonoBehaviour, MMEventListener<EquipmentEvent>
    {
        private CompassPro compassPro;

        private void Start()
        {
            compassPro = GetComponent<CompassPro>();
            if (compassPro == null) UnityEngine.Debug.LogError("CompassPro component not found on this GameObject.");
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(EquipmentEvent eventType)
        {
            if (eventType.EventType == EquipmentEventType.EquipScanner)
            {
                compassPro.showOnScreenIndicators = true;
                compassPro.showOffScreenIndicators = true;
                compassPro.UpdateSettings();
            }
            else if (eventType.EventType == EquipmentEventType.SwitchFromScanner)
            {
                compassPro.showOnScreenIndicators = false;
                compassPro.showOffScreenIndicators = false;
                compassPro.UpdateSettings();
            }
        }
    }
}