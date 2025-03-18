using Domains.Player.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

public class GlobalFeedbackManager : MonoBehaviour, MMEventListener<UpgradeEvent>
{
    [FormerlySerializedAs("UpgradeFeedbacks")] [SerializeField] MMFeedbacks upgradeFeedbacks;
    [SerializeField] MMFeedbacks upgradeFailedFeedbacks;

    
    private void OnEnable()
    {
        this.MMEventStartListening();
    }
    
    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    public void OnMMEvent(UpgradeEvent eventType)
    {
        if (eventType.EventType == UpgradeEventType.UpgradePurchased)
            upgradeFeedbacks.PlayFeedbacks();
        else if (eventType.EventType == UpgradeEventType.UpgradeFailed)
            upgradeFailedFeedbacks.PlayFeedbacks();
        
    }
}
