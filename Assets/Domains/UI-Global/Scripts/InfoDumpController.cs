using System.Collections;
using Domains.UI_Global.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.UI_Global.Scripts
{
    public class InfoDumpController : MonoBehaviour, MMEventListener<UIEvent>
    {
        [SerializeField] private bool shouldShowInfoDump = true;
        private bool _isPaused;
        private CanvasGroup canvasGroup;

        public InfoDumpController(bool isPaused)
        {
            _isPaused = isPaused;
        }

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                UnityEngine.Debug.LogError("InfoDumpController: No CanvasGroup found on this GameObject.");
                return;
            }

            if (shouldShowInfoDump)
                StartCoroutine(DelayedShowInfoDump());
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
            if (eventType.EventType == UIEventType.CloseUI)
            {
                shouldShowInfoDump = false;
                HideInfoDump();
            }
        }

        private IEnumerator DelayedShowInfoDump()
        {
            yield return null; // Wait one frame
            ShowInfoDump();
        }

        private void ShowInfoDump()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                _isPaused = true;
                Time.timeScale = 0;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                UnityEngine.Debug.LogError("InfoDumpController: No CanvasGroup found on this GameObject.");
            }
        }

        private void HideInfoDump()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                _isPaused = false;
                Time.timeScale = 1;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                UnityEngine.Debug.LogError("InfoDumpController: No CanvasGroup found on this GameObject.");
            }
        }
    }
}