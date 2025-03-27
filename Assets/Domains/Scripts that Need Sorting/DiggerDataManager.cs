using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Player.Events;
using Domains.UI_Global.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

public class DiggerDataManager : MonoBehaviour, MMEventListener<DiggerEvent>
{
    public DiggerMasterRuntime diggerMasterRuntime;

    public MMFeedbacks deleteAllDataFeedbacks;
    public MMFeedbacks saveDataFeedbacks;

    public DiggerSystem[] diggerSystems;

    public bool autoSave = true;
    [FormerlySerializedAs("doNotPersist")] public bool forceDeleteOnQuit;
    public static DiggerDataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (diggerMasterRuntime == null)
        {
            diggerMasterRuntime = FindFirstObjectByType<DiggerMasterRuntime>();
            if (diggerMasterRuntime == null)
                Debug.LogError("DiggerDataManager: No DiggerMasterRuntime found in scene!");
        }
    }


    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    private void OnApplicationQuit()
    {
        if (forceDeleteOnQuit) DeleteAllDiggerData();

        if (autoSave) SaveDiggerData();
    }

    public void OnMMEvent(DiggerEvent eventType)
    {
        switch (eventType.EventType)
        {
            case DiggerEventType.Persist:
                SaveDiggerData();
                break;
            case DiggerEventType.Delete:
                DeleteAllDiggerData();
                break;
        }
    }


    public void SaveDiggerData()
    {
        saveDataFeedbacks?.PlayFeedbacks();
        diggerMasterRuntime.PersistAll();

        Debug.Log("Digger data saved.");
    }

    public void DeleteAllDiggerData()
    {
        deleteAllDataFeedbacks?.PlayFeedbacks();
        diggerMasterRuntime.DeleteAllPersistedData();
        AlertEvent.Trigger(AlertType.DeletingDiggerData, "Digger data deleted.");

        Debug.Log("Digger data deleted.");
    }
}