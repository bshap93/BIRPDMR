using Domains.Scene.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Scripts
{
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
                var isPaused = _canvasGroup.alpha == 0;

                _canvasGroup.alpha = isPaused ? 1 : 0;
                _canvasGroup.interactable = isPaused;
                _canvasGroup.blocksRaycasts = isPaused;

                // ✅ Control the mouse cursor visibility and lock state
                Cursor.visible = isPaused;
                Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

                UnityEngine.Debug.Log("CanvasGroup settings: alpha=" + _canvasGroup.alpha +
                                      ", interactable=" + _canvasGroup.interactable +
                                      ", blocksRaycasts=" + _canvasGroup.blocksRaycasts);
            }
        }
    }
}