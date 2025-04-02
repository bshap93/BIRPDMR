using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using Domains.UI_Global.Events;
using UnityEngine;

namespace Domains.Items
{
    public class BeaconInteractable : MonoBehaviour, IInteractable
    {
        public string UniqueID;

        public void Interact()
        {
            CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, 800);
            AlertEvent.Trigger(AlertReason.CreditsAdded, "800 Credits Added to your account",
                "Beacon Interacted", null, null, Color.white);
        }

        public void ShowInteractablePrompt()
        {
        }

        public void HideInteractablePrompt()
        {
        }
    }
}