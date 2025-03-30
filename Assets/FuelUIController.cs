using Domains.UI_Global.Events;
using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;

public class FuelUIController : MonoBehaviour, MMEventListener<UIEvent>
{
    private CanvasGroup _canvasGroup;

    private bool _isPaused;

    private ProgressBar fuelRemainingRadial;

    public FuelUIController(bool isPaused)
    {
        _isPaused = isPaused;
    }

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        CloseFuelUI();
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
        if (eventType.EventType == UIEventType.OpenFuelConsole)
            OpenFuelUI();
        else if (eventType.EventType == UIEventType.CloseFuelConsole) CloseFuelUI();
    }

    public void CloseFuelUI()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        _isPaused = false;
        Time.timeScale = 1;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenFuelUI()
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