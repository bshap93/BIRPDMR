using Domains.Player.Events;
using Domains.UI_Global.Events;

namespace Domains.Items.Scripts
{
    public class BioContainerInteractable : InteractableObjective
    {
        public override void Interact()
        {
            if (hasBeenInteractedWith) return;
            CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, RewardAmount);
            AlertEvent.Trigger(AlertReason.CreditsAdded, $"{RewardAmount} Credits Added to your account",
                "BioContainer Scanned");

            interactFeedbacks?.PlayFeedbacks();
            hasBeenInteractedWith = true;
        }
    }
}