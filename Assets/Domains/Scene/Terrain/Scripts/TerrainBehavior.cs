using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Scene.Terrain.Scripts
{
    [Serializable]
    public class TerrainLayerChoices
    {
        public int terrainLayerIndex;
        public int terrainToUseInstead; // or whatever type you need
    }

    [Serializable]
    public class DefaultLayerAboveDepth
    {
        public float playerDepth;
        public int defaultLayerIndex;
        public int[] alternateAcceptableLayerIndices;
    }

    [CreateAssetMenu(fileName = "TerrainBehavior", menuName = "Scriptable Objects/TerrainBehavior")]
    public class TerrainBehavior : ScriptableObject
    {
        [FormerlySerializedAs("statEntries")] public List<TerrainLayerChoices> terrainChoices;
        public List<DefaultLayerAboveDepth> defaultLayerAboveDepths;
    }
}