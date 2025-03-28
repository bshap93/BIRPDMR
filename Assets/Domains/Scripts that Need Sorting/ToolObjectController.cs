using Domains.Gameplay.Mining.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace Domains.Scripts_that_Need_Sorting
{
    public enum ToolType
    {
        Shovel,
        Pickaxe,
        Drill
    }

    public enum ToolIteration
    {
        Smallest,
        SecondSmallest
    }

    public class ToolObjectController : MonoBehaviour, MMEventListener<ToolEvent>
    {
        public UnityEvent OnToolUseAction;


        public ToolType toolType;

        public ToolIteration toolIteration;


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ToolEvent eventType)
        {
            if (eventType.ToolType == ToolType.Shovel)
                switch (eventType.ToolIteration)
                {
                    case ToolIteration.Smallest:
                        OnToolUseAction.Invoke();
                        break;
                    case ToolIteration.SecondSmallest:
                        UnityEngine.Debug.Log("Tool used with second smallest iteration");
                        OnToolUseAction.Invoke();
                        break;
                }
        }


        public void OnToolUse()
        {
            UnityEngine.Debug.Log("Tool used");
        }
    }
}