using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using Domains.SaveLoad;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Domains.Input.Scripts
{
    public class ButtonActivatedWithAction : MonoBehaviour, IInteractable
    {
        public UnityEvent OnActivation;
        public ButtonPromptWithAction ButtonPromptPrefab;

        public Vector3 promptTransformOffset;
        public Vector3 promptRotationOffset;

        public MMFeedbacks activationFeedback;

        [FormerlySerializedAs("PromptActionText")]
        public string PromptActionStr = "Interact";

        public Color PromptTextColor = Color.white;

        [FormerlySerializedAs("PromptKeyText")]
        public string PromptKeyStr = "E";

        private ButtonPromptWithAction _buttonPrompt;

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

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) ShowInteractablePrompt();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) HideInteractablePrompt();
        }

        public void Interact()
        {
            ActivateButton();
        }

        public void ShowInteractablePrompt()
        {
            if (_buttonPrompt != null) _buttonPrompt.Show(PromptKeyStr, PromptActionStr);
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
            }
        }


        public void TriggerSave()
        {
            SaveLoadEvent.Trigger(SaveLoadEventType.Save);
        }

        public void TriggerHealthRestore()
        {
            HealthEvent.Trigger(HealthEventType.RecoverHealth, 100);
        }

        public void HidePrompt()
        {
            if (_buttonPrompt != null) _buttonPrompt.Hide();
        }
    }
}