using Domains.Player.Events;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class PointedAtUI : MonoBehaviour, MMEventListener<PointedObjectEvent>
{
    public TMP_Text textPointedAt;

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    public void OnMMEvent(PointedObjectEvent eventType)
    {
        if (eventType.eventType == PointedObjectEventType.PointedObjectChanged) textPointedAt.text = eventType.name;
    }
}