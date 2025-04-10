using System.Collections.Generic;
using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Scripts_that_Need_Sorting
{
    public class TerrainMineable : MonoBehaviour, IMinable, MMEventListener<PlayerPositionEvent>
    {
        [FormerlySerializedAs("textureDetector")] [SerializeField]
        private TerrainLayerDetector terrainLayerDetector;

        [SerializeField] private MMFeedbacks failHitFeedbacks;
        [SerializeField] private GameObject failHitParticles;
        [SerializeField] private int[] hardnessLevels;
        [SerializeField] private int[] layerIndices;

        [SerializeField] private int currentHardnessLevel;

        [SerializeField] private int currentLayerIndex;

        [SerializeField] private float[] depthThresholds;
        [SerializeField] private int[] layerIndexPerDepth;


        private void Start()
        {
            if (hardnessLevels.Length != layerIndices.Length)
            {
                UnityEngine.Debug.LogError("Hardness levels and layer indices must have the same length.");
                return;
            }

            var layerHardnessMap = new Dictionary<int, int>();
            for (var i = 0; i < layerIndices.Length; i++) layerHardnessMap[layerIndices[i]] = hardnessLevels[i];
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
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

        public void OnMMEvent(PlayerPositionEvent eventType)
        {
            if (eventType.EventType == PlayerPositionEventType.ReportDepth)
            {
                if (eventType.Position.y > depthThresholds[0])
                    currentLayerIndex = layerIndexPerDepth[0];
                else if (eventType.Position.y > depthThresholds[1])
                    currentLayerIndex = layerIndexPerDepth[1];
                else if (eventType.Position.y > depthThresholds[2])
                    currentLayerIndex = layerIndexPerDepth[2];
                else
                    currentLayerIndex = layerIndexPerDepth[3];
            }
        }

        public int GetCurrentLayerIndexPerHeight()
        {
            return currentLayerIndex;
        }
    }
}