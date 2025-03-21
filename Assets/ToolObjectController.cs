using System;
using Domains.Gameplay.Mining.Events;
using MoreMountains.Tools;
using Unity.VisualScripting;
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


    private void OnEnable()
    {
        this.MMEventStartListening<ToolEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<ToolEvent>();
    }


    public ToolType toolType;

    public ToolIteration toolIteration;


    public void OnToolUse()
    {
        Debug.Log("Tool used");
    }

    public void OnMMEvent(ToolEvent eventType)
    {
        if (eventType.ToolType == ToolType.Shovel)
            switch (eventType.ToolIteration)
            {
                case ToolIteration.Smallest:
                    Debug.Log("Tool used with smallest iteration");
                    OnToolUseAction.Invoke();
                    break;
                case ToolIteration.SecondSmallest:
                    Debug.Log("Tool used with second smallest iteration");
                    OnToolUseAction.Invoke();
                    break;
            }
    }
}