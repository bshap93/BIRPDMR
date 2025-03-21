using Digger.Modules.Runtime.Sources;
using UnityEngine;

public class DiggerMasterRuntimeHelper : MonoBehaviour

{
    [SerializeField] private DiggerMasterRuntime diggerMasterRuntime;
    [SerializeField] private Terrain terrain;

    private void Start()
    {
        if (!Application.isEditor) diggerMasterRuntime.SetupRuntimeTerrain(terrain);
    }


    public void ForceDeleteAllPersistedData()
    {
        if (diggerMasterRuntime == null)
        {
            Debug.LogError("DiggerMasterRuntime is not assigned.");
            return;
        }


        diggerMasterRuntime.DeleteAllPersistedData();

        diggerMasterRuntime.SetupRuntimeTerrain(terrain);
    }
}