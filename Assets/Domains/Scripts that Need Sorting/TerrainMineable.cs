using System.Collections.Generic;
using Domains.Gameplay.Mining.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

public class TerrainMineable : MonoBehaviour, IMinable
{
    [FormerlySerializedAs("textureDetector")] [SerializeField] private TerrainLayerDetector terrainLayerDetector;
    [SerializeField] private MMFeedbacks failHitFeedbacks;
    [SerializeField] private GameObject failHitParticles;
    [SerializeField] private int[] hardnessLevels;
    [SerializeField] private int[] layerIndices;

    [SerializeField] private int currentHardnessLevel;

    [SerializeField] private int currentLayerIndex;

    private void Start()
    {
        if (hardnessLevels.Length != layerIndices.Length)
        {
            Debug.LogError("Hardness levels and layer indices must have the same length.");
            return;
        }

        var layerHardnessMap = new Dictionary<int, int>();
        for (var i = 0; i < layerIndices.Length; i++) layerHardnessMap[layerIndices[i]] = hardnessLevels[i];
    }

    public void MinableMineHit()
    {
        // Give the correct feedbacks
        // for a given layer
    }

    public void MinableFailHit(Vector3 hitPoint)
    {
        failHitFeedbacks?.PlayFeedbacks();

        if (failHitParticles != null)
        {
            var fx = Instantiate(failHitParticles, hitPoint, Quaternion.identity);
            Destroy(fx, 2f);
        }
    }

    public int GetCurrentMinableHardness()
    {
        // Change this later
        return 0;
    }
}