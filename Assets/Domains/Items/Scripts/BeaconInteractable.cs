using Domains.Player.Events;
using Domains.UI_Global.Events;
using UnityEngine;

namespace Domains.Items.Scripts
{
    public class BeaconInteractable : InteractableObjective
    {
        public override void Interact()
        {
            if (hasBeenInteractedWith) return;
            CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, 800);
            AlertEvent.Trigger(AlertReason.CreditsAdded, "800 Credits Added to your account",
                "Beacon Interacted", null, null, Color.white);

            interactFeedbacks?.PlayFeedbacks();
            hasBeenInteractedWith = true;
        }
    }
}