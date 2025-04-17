using CompassNavigatorPro;
using Domains.Gameplay.Mining.Scripts;
using MoreMountains.Feedbacks;
using Plugins.Kronnect.CompassNavigatorPro.Scripts;
using UnityEngine;

namespace Domains.Items.Scripts
{
    public abstract class InteractableObjective : MonoBehaviour, IInteractable
    {
        public string UniqueID;

        public int RewardAmount;

        [SerializeField] protected bool hasBeenInteractedWith;

        public MMFeedbacks interactFeedbacks;


        protected CompassPro compassPro;

        protected CompassProPOI compassProPOI;

        protected void Start()
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

        public abstract void Interact();

        public void ShowInteractablePrompt()
        {
        }

        public void HideInteractablePrompt()
        {
            if (compassProPOI != null) compassProPOI.ToggleIndicatorVisibility(false);
        }
    }
}