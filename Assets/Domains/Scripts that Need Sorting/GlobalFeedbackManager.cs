using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

public class GlobalFeedbackManager : MonoBehaviour, MMEventListener<UpgradeEvent>, MMEventListener<PlayerStatusEvent>
{
    [FormerlySerializedAs("UpgradeFeedbacks")] [SerializeField]
    private MMFeedbacks upgradeFeedbacks;

    [SerializeField] private MMFeedbacks upgradeFailedFeedbacks;
    [SerializeField] private MMFeedbacks outOfStaminaFeedbacks;


    private void OnEnable()
    {
        this.MMEventStartListening<UpgradeEvent>();
        this.MMEventStartListening<PlayerStatusEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<UpgradeEvent>();
        this.MMEventStopListening<PlayerStatusEvent>();
    }

    public void OnMMEvent(UpgradeEvent eventType)
    {
        if (eventType.EventType == UpgradeEventType.UpgradePurchased)
            upgradeFeedbacks.PlayFeedbacks();
        else if (eventType.EventType == UpgradeEventType.UpgradeFailed)
            upgradeFailedFeedbacks.PlayFeedbacks();
    }

    public void OnMMEvent(PlayerStatusEvent eventType)
    {
        if (eventType.EventType == PlayerStatusEventType.OutOfStamina)
            outOfStaminaFeedbacks.PlayFeedbacks();
    }
}