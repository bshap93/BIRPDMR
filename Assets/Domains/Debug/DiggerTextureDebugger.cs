using Digger.Modules.Core.Sources;
using Unity.Mathematics;
using UnityEngine;

namespace Domains.Debug
{
    public class DiggerTextureDebugger : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private KeyCode checkKey = KeyCode.T;
        [SerializeField] private LayerMask playerMask;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(checkKey))
            {
                if (!mainCamera) mainCamera = Camera.main;

                if (mainCamera == null)
                {
                    UnityEngine.Debug.LogWarning("No main camera assigned or found.");
                    return;
                }

                var notLayerMask = ~playerMask;

                var ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 5f, notLayerMask))
                {
                    UnityEngine.Debug.Log($"Hit: {hit.collider.name}");

                    // Check if the hit object is a Digger chunk
                    if (hit.collider.CompareTag($"DiggerChunk"))
                        UnityEngine.Debug.Log("Hit a Digger chunk.");
                    else
                        UnityEngine.Debug.Log("Hit something else.");

                    // Get the VoxelChunk and check the texture index
                    {
                        var chunkObject = hit.collider.GetComponentInParent<ChunkObject>();
                        if (chunkObject != null)
                        {
                            var voxelChunk = chunkObject.GetComponentInParent<Chunk>()?.VoxelChunk;
                            if (voxelChunk != null)
                            {
                                var index = GetDominantTextureIndex(voxelChunk, hit.point);
                                UnityEngine.Debug.Log($"Digger texture index at hit point: {index}");
                            }
                            else
                            {
                                UnityEngine.Debug.Log("Hit a chunk, but no VoxelChunk found.");
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.Log("Raycast hit something, but not a Digger chunk.");
                        }
                    }
                }
            }
        }

        private int GetDominantTextureIndex(VoxelChunk voxelChunk, Vector3 worldHitPoint)
        {
            float3 local = voxelChunk.Digger.transform.InverseTransformPoint(worldHitPoint);
            Vector3 relative = local - voxelChunk.WorldPosition;

            Vector3 chunkSize = voxelChunk.SizeOfMesh * voxelChunk.HeightmapScale;
            var normX = Mathf.Clamp01(relative.x / chunkSize.x);
            var normZ = Mathf.Clamp01(relative.z / chunkSize.z);

            var width = voxelChunk.AlphamapArraySize.x;
            var height = voxelChunk.AlphamapArraySize.y;
            var layerCount = voxelChunk.AlphamapArraySize.z;

            var mapX = Mathf.Clamp(Mathf.FloorToInt(normX * width), 0, width - 1);
            var mapZ = Mathf.Clamp(Mathf.FloorToInt(normZ * height), 0, height - 1);

            var bestIndex = 0;
            var maxWeight = -1f;

            for (var layer = 0; layer < layerCount; layer++)
            {
                var index = layer * width * height + mapZ * width + mapX;
                var weight = voxelChunk.AlphamapArray[index];
                if (weight > maxWeight)
                {
                    maxWeight = weight;
                    bestIndex = layer;
                }
            }

            return bestIndex;
        }
    }
}