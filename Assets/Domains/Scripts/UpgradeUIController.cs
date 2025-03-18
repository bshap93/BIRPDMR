using Domains.UI_Global.Events;
using MoreMountains.Tools;
using UnityEngine;

public class UpgradeUIController : MonoBehaviour, MMEventListener<UIEvent>
{
    private CanvasGroup _canvasGroup;

    private bool _isPaused;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        CloseUpgradeUI();
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
        if (eventType.EventType == UIEventType.OpenVendorConsole)
            OpenUpgradeUI();
        else if (eventType.EventType == UIEventType.CloseVendorConsole) CloseUpgradeUI();
    }


    public void CloseUpgradeUI()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _isPaused = false;
        Time.timeScale = 1;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenUpgradeUI()
    {
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;

        _isPaused = true;
        Time.timeScale = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}