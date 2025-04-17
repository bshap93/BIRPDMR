using Domains.Input.Scripts;
using Domains.Scene.Events;
using UnityEngine;

namespace Domains.Gameplay.Managers.Scripts
{
    public class PauseManager : MonoBehaviour
    {
        private bool _isPaused;

        private void Update()
        {
            if (CustomInputBindings.IsPausePressed())
            {
                _isPaused = !_isPaused;
                Time.timeScale = _isPaused ? 0 : 1;
                AudioListener.pause = _isPaused;


                SceneEvent.Trigger(SceneEventType.TogglePauseScene);
            }
        }
    }
}