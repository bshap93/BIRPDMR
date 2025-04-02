using Domains.UI_Global.Events;
using MoreMountains.Tools;
using UnityEngine;

public class InfoDumpController : MonoBehaviour, MMEventListener<UIEvent>
{
    private CanvasGroup canvasGroup;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) Debug.LogError("InfoDumpController: No CanvasGroup found on this GameObject.");
        
        
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    public void OnMMEvent(UIEvent eventType)
    {
        if (eventType.EventType == UIEventType.CloseUI) HideInfoDump();
    }

    private void ShowInfoDump()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            Debug.LogError("InfoDumpController: No CanvasGroup found on this GameObject.");
        }
    }

    private void HideInfoDump()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("InfoDumpController: No CanvasGroup found on this GameObject.");
        }
    }
}