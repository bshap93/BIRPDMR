using Domains.UI_Global.Events;
using Michsky.MUIP;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

public class AlertUIController : MonoBehaviour, MMEventListener<AlertEvent>
{
    private NotificationManager _notificationManager;
    [SerializeField] private MMFeedbacks normalAlertFeedbacks;

    private void Awake()
    {
        _notificationManager = GetComponentInChildren<NotificationManager>();
    }

    private void Start()
    {
        AlertEvent.Trigger(AlertType.Test, "This is a test alert message.", "Test Alert", null);
    }


    public void ShowAlert(AlertEvent evt)
    {
        _notificationManager.title = evt.AlertTitle;
        _notificationManager.description = evt.AlertMessage;
        _notificationManager.icon = evt.AlertIcon;
        _notificationManager.UpdateUI();


        _notificationManager.Open();
    }

    public void HideAlert()
    {
        _notificationManager.Close();
    }

    public void OnMMEvent(AlertEvent eventType)
    {
        ShowAlert(eventType);
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