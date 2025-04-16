using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Gameplay.Vendor.Train
{
    public enum TrainSegmentState
    {
        Docked,
        Outbound,
        HasLeftArea,
        Inbound
    }

    public class TrainSegmentController : MonoBehaviour
    {
        public MMFeedbacks sendoffFeedbacks;
        private bool isDocked;
        private bool isNotBlocked;
        private DOTweenAnimation sendoffAnimation;


        // Awake is called when the script instance is being loaded
        private void Awake()
        {
            isDocked = true;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            sendoffAnimation = GetComponent<DOTweenAnimation>();
            if (sendoffAnimation == null)
            {
                UnityEngine.Debug.LogError("DOTweenAnimation component not found on this GameObject.");
                return;
            }

            sendoffAnimation.DOPause();
        }

        public void SendOff()
        {
            if (isDocked)
            {
                isDocked = false;

                sendoffFeedbacks?.PlayFeedbacks();


                sendoffAnimation.DOPlay();
            }
        }

        // Add this method to your controller if you don't have it already
        public void OnExitArea()
        {
        }
    }
}