using CompassNavigatorPro;
using Domains.Gameplay.Equipment.Events;
using Domains.Scripts_that_Need_Sorting;
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
            if (compassPro == null)
            {
                UnityEngine.Debug.LogError("CompassPro component not found on this GameObject.");
                return;
            }

            compassPro.showOnScreenIndicators = false;
            compassPro.UpdateSettings();
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
            if (eventType.ToolType == ToolType.Scanner)
            {
                compassPro.showOnScreenIndicators = true;
                compassPro.UpdateSettings();
            }
            else if (eventType.ToolType == ToolType.Pickaxe || eventType.ToolType == ToolType.Shovel)
            {
                compassPro.showOnScreenIndicators = false;

                compassPro.UpdateSettings();
            }
        }
    }
}