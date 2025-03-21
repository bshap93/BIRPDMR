using System;
using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using Domains.Player.Events;
using Domains.Scene.Scripts;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class DiggerDataManager : MonoBehaviour, MMEventListener<DiggerEvent>
{
    public static DiggerDataManager Instance { get; private set; }

    public DiggerMasterRuntime diggerMasterRuntime;

    public MMFeedbacks deleteAllDataFeedbacks;
    public MMFeedbacks saveDataFeedbacks;

    public DiggerSystem[] diggerSystems;

    public bool autoSave = true;
    [FormerlySerializedAs("doNotPersist")] public bool forceDeleteOnQuit = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

        Debug.Log("Digger data deleted.");
    }


    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
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

    private void OnApplicationQuit()
    {
        if (forceDeleteOnQuit) DeleteAllDiggerData();

        if (autoSave) SaveDiggerData();
    }
}