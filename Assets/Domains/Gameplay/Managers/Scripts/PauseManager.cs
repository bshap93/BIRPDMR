using System.Collections.Generic;
using Domains.Input.Scripts;
using Domains.Scene.Events;
using UnityEngine;

namespace Domains.Gameplay.Managers.Scripts
{
    public class PauseManager : MonoBehaviour
    {
        [SerializeField] private AudioSource uiButtonAudioSource;
        private List<AudioSource> _audioSources = new();

        private bool _isPaused;

        private void Update()
        {
            if (CustomInputBindings.IsPausePressed())
            {
                _isPaused = !_isPaused;
                Time.timeScale = _isPaused ? 0 : 1;
                if (_isPaused)
                {
                    _audioSources = new List<AudioSource>(FindObjectsByType<AudioSource>(FindObjectsSortMode.None));
                    foreach (var audioSource in _audioSources)
                        if (audioSource != null && audioSource != uiButtonAudioSource)
                            audioSource.Pause();
                }
                else
                {
                    foreach (var audioSource in _audioSources)
                        if (audioSource != null && audioSource != uiButtonAudioSource)
                            audioSource.UnPause();
                }


                SceneEvent.Trigger(SceneEventType.TogglePauseScene);
            }
        }
    }
}