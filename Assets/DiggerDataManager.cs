using System;
using Digger.Modules.Runtime.Sources;
using Domains.Player.Events;
using Domains.Scene.Scripts;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Unity.VisualScripting;
using UnityEngine;

public class DiggerDataManager : MonoBehaviour, MMEventListener<DiggerEvent>
{
    public static DiggerDataManager Instance { get; private set; }

    public DiggerMasterRuntime diggerMasterRuntime;

    public MMFeedbacks deleteAllDataFeedbacks;
    public MMFeedbacks saveDataFeedbacks;

    public bool autoSave = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ResetDiggerData()
    {
        deleteAllDataFeedbacks?.PlayFeedbacks();
        diggerMasterRuntime.DeleteAllPersistedData();
    }


    public void SaveDiggerData()
    {
        saveDataFeedbacks?.PlayFeedbacks();
        diggerMasterRuntime.PersistAll();

        Debug.Log("Digger data saved.");
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
        throw new NotImplementedException();
    }

    // On App Quit
    // private void OnApplicationQuit()
    // {
    //     SaveDiggerData();
    //
    //     // SaveManager.Instance.SaveAll();
    // }
}