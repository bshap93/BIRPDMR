using Domains.Gameplay.Mining.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

public enum ToolType
{
    Shovel
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
                    Debug.Log("Tool used with second smallest iteration");
                    OnToolUseAction.Invoke();
                    break;
            }
    }


    public void OnToolUse()
    {
        Debug.Log("Tool used");
    }
}