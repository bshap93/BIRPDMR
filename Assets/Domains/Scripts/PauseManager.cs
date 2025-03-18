using Domains.Scene.Events;
using UnityEngine;

namespace Domains.Scripts
{
    public class PauseManager : MonoBehaviour
    {
        private bool _isPaused;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                _isPaused = !_isPaused;
                Time.timeScale = _isPaused ? 0 : 1;

                SceneEvent.Trigger(SceneEventType.TogglePauseScene);
            }
        }
    }
}