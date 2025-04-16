using CompassNavigatorPro;
using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using Plugins.Kronnect.CompassNavigatorPro.Scripts;
using UnityEngine;

namespace Domains.Items
{
    public class BeaconInteractable : MonoBehaviour, IInteractable
    {
        public string UniqueID;

        [SerializeField] private bool hasBeenInteractedWith;

        public MMFeedbacks interactFeedbacks;

        private CompassPro compassPro;

        private CompassProPOI compassProPOI;

        private void Start()
        {
            compassProPOI = GetComponent<CompassProPOI>();
            if (compassProPOI != null)
            {
                compassProPOI.ToggleIndicatorVisibility(false);


                UnityEngine.Debug.Log("POI visibility set to always hidden");
            }

            compassPro = FindFirstObjectByType<CompassPro>();
            if (compassPro != null) compassPro.UpdateSettings();
        }

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