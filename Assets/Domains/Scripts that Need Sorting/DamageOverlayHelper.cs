using System.Collections;
using System.Collections.Generic;
using Domains.Player.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

public class DamageOverlayHelper : MonoBehaviour, MMEventListener<HealthEvent>
{
    private MMSpringImageAlpha _damageOverlay;
    public float targetAlpha = 0.3f;

    private void Start()
    {
        _damageOverlay = GetComponent<MMSpringImageAlpha>();
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }


    public void OnMMEvent(HealthEvent eventType)
    {
        if (eventType.EventType == HealthEventType.ConsumeHealth)
            StartCoroutine(AnimateDamageOverlay());
    }

    private IEnumerator AnimateDamageOverlay()
    {
        _damageOverlay.MoveToAdditive(targetAlpha);
        yield return new WaitForSeconds(0.5f);
        _damageOverlay.MoveToSubtractive(targetAlpha);
    }
}