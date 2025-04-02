using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using UnityEngine;

namespace Domains.Items
{
    public class BeaconInteractable : MonoBehaviour, IInteractable
    {
        public string UniqueID;

        public void Interact()
        {
            CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, 800);
        }

        public void ShowInteractablePrompt()
        {
        }

        public void HideInteractablePrompt()
        {
        }
    }
}