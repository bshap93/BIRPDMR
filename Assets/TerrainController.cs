using Domains.Scene.Terrain.Scripts;
using UnityEngine;

public class TerrainController : MonoBehaviour
{
    [SerializeField] public TerrainBehavior terrainBehavior;

    public static TerrainController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
}