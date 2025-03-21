using Digger.Modules.Runtime.Sources;
using Sirenix.OdinInspector;
using UnityEngine;

public class DiggerMasterRuntimeHelper : MonoBehaviour

{
    [SerializeField] private DiggerMasterRuntime diggerMasterRuntime;
    [SerializeField] private Terrain terrain;

    private void Start()
    {
        diggerMasterRuntime.SetupRuntimeTerrain(terrain);
    }


    [Button("Force Delete Persisted Data", ButtonSizes.Medium)]
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