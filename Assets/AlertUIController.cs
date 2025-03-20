using Domains.UI.Events;
using Michsky.MUIP;
using MoreMountains.Tools;
using UnityEngine;

public class AlertUIController : MonoBehaviour, MMEventListener<AlertEvent>
{
    private NotificationManager _notificationManager;

    private void Awake()
    {
        _notificationManager = GetComponentInChildren<NotificationManager>();
    }

    private void Start()
    {
        ShowAlert("Test Alert");
    }


    public void ShowAlert(string message)
    {
        _notificationManager.Open();
    }

    public void HideAlert()
    {
        _notificationManager.Close();
    }

    public void OnMMEvent(AlertEvent eventType)
    {
        if (eventType.AlertType == AlertType.InventoryFull)
            ShowAlert(eventType.AlertMessage);
        else
            HideAlert();
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }
}