using Domains.Gameplay.Mining.Scripts;
using Domains.Mining.Scripts;
using Domains.SaveLoad;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;

namespace Domains.Input.Scripts
{
    public class ButtonActivated : MonoBehaviour, IInteractable
    {
        public UnityEvent OnActivation;
        public ButtonPrompt ButtonPromptPrefab;

        public Vector3 promptTransformOffset;
        public Vector3 promptRotationOffset;

        public MMFeedbacks activationFeedback;
        private ButtonPrompt _buttonPrompt;

        private void Start()
        {
            if (ButtonPromptPrefab != null)
            {
                var promptPosition = transform.position + promptTransformOffset;
                var promptRotation = Quaternion.Euler(promptRotationOffset);
                _buttonPrompt = Instantiate(ButtonPromptPrefab, promptPosition, promptRotation, transform);
                _buttonPrompt.Initialization();
                _buttonPrompt.Hide();
            }
        }

        public void Interact()
        {
            ActivateButton();
        }

        public void ShowInteractablePrompt()
        {
            if (_buttonPrompt != null) _buttonPrompt.Show();
        }

        public void HideInteractablePrompt()
        {
            if (_buttonPrompt != null) _buttonPrompt.Hide();
        }

        private void ActivateButton()
        {
            if (OnActivation != null)
            {
                OnActivation.Invoke();
                activationFeedback?.PlayFeedbacks();
                UnityEngine.Debug.Log("Button Activated!");
            }
        }


        public void TriggerSave()
        {
            UnityEngine.Debug.Log("Triggering save");
            SaveLoadEvent.Trigger(SaveLoadEventType.Save);
        }
    }
}