using Domains.Scene.Events;
using MoreMountains.Tools;
using UnityEngine;

public class PauseOverlay : MonoBehaviour, MMEventListener<SceneEvent>
{
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    public void OnMMEvent(SceneEvent eventType)
    {
        if (eventType.EventType == SceneEventType.TogglePauseScene)
        {
            _canvasGroup.alpha = _canvasGroup.alpha == 0 ? 1 : 0;
            _canvasGroup.interactable = !_canvasGroup.interactable;
            _canvasGroup.blocksRaycasts = !_canvasGroup.blocksRaycasts;

            Debug.Log("CanvasGroup settings: alpha=" + _canvasGroup.alpha + ", interactable=" +
                      _canvasGroup.interactable + ", blocksRaycasts=" + _canvasGroup.blocksRaycasts);
        }
    }
}