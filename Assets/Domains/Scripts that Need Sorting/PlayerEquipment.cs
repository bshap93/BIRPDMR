using UnityEngine;

namespace Domains.Scripts_that_Need_Sorting
{
    public class PlayerEquipment : MonoBehaviour
    {
        public static PlayerEquipment Instance;

        public GameObject currentToolGraphicalObject;

        public ToolType currentToolType;
        public ToolIteration currentToolIteration;

        private void Awake()
        {
            Instance = this;
        }
    }
}