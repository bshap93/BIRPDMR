using System;
using Digger.Modules.Runtime.Sources;
using Domains.Player.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
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

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) // Press F5 to force save
        {
            
            saveDataFeedbacks?.PlayFeedbacks();
            
            Debug.Log("PersistAll called at " + DateTime.Now);


            diggerMasterRuntime.PersistAll();

        }
        
        if (UnityEngine.Input.GetKeyDown(KeyCode.F6)) // Press F6 to force delete all data
        {
            deleteAllDataFeedbacks?.PlayFeedbacks();
            diggerMasterRuntime.DeleteAllPersistedData();
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

    public void OnMMEvent(DiggerEvent eventType)
    {
        throw new System.NotImplementedException();
    }
    
    private void OnApplicationQuit()
    {
        if (autoSave)
        {
            
        Debug.Log("Application quitting - persisting Digger data");
        diggerMasterRuntime.PersistAll();
        }
    }
}
