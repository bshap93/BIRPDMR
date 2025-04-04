using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Items
{
    public class BeaconInteractable : MonoBehaviour, IInteractable
    {
        public string UniqueID;

        [SerializeField] private bool hasBeenInteractedWith;

        public MMFeedbacks interactFeedbacks;

        public void Interact()
        {
            if (hasBeenInteractedWith) return;
            CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, 800);
            AlertEvent.Trigger(AlertReason.CreditsAdded, "800 Credits Added to your account",
                "Beacon Interacted", null, null, Color.white);

            interactFeedbacks?.PlayFeedbacks();
            hasBeenInteractedWith = true;
        }

        public void ShowInteractablePrompt()
        {
        }

        public void HideInteractablePrompt()
        {
        }
    }
}