using System.Collections.Generic;
using Digger.Modules.Core.Sources;
using Unity.Mathematics;
using UnityEngine;

namespace Digger.Demo
{
    public class TextureDetector : MonoBehaviour
    {
        private static readonly List<Vector4> uvs = new();

        [Header("Targeted texture (will be filled with the name of the texture you are targetting in Play mode)")]
        public string texture = "";

        public int textureIndex = -1;

        private DiggerMaster diggerMaster;

        protected void Start()
        {
            diggerMaster = FindObjectOfType<DiggerMaster>();
        }

        protected void Update()
        {
            if (Physics.Raycast(transform.position, transform.forward, out var hit, 500, 1 << diggerMaster.Layer))
            {
                Debug.DrawLine(transform.position, hit.point, Color.green);
                var index = GetTextureIndex(hit, out var terrain);

                if (terrain != null && index >= 0)
                {
                    var layers = terrain.terrainData.terrainLayers;
                    if (index < layers.Length)
                    {
                        texture = $"name: {layers[index].name} | index: {index}";
                        textureIndex = index;
                    }
                    else
                    {
                        texture = "Invalid texture index";
                        textureIndex = -1;
                    }
                }
                else if (hit.collider.GetComponent<ChunkObject>() != null)
                {
                    texture = "Chunk hit — no texture index (fallback)";
                    textureIndex = 1; // or a default value like 0
                }
                else
                {
                    texture = "No terrain or chunk hit";
                    textureIndex = -1;
                }
            }
        }

        private static bool TryGetTexcoord(Mesh mesh, int baseVertexIndex, int channel, out float4 coord)
        {
            uvs.Clear();
            mesh.GetUVs(channel, uvs);

            if (baseVertexIndex < 0 || baseVertexIndex >= uvs.Count)
            {
                Debug.LogWarning($"Invalid UV index: {baseVertexIndex} for channel {channel}. UV count: {uvs.Count}");
                coord = float4.zero;
                return false;
            }

            coord = uvs[baseVertexIndex];
            return true;
        }

        /// <summary>
        ///     Convert a given RaycastHit to a texture index
        /// </summary>
        /// <param name="hit">Physics RaycastHit used to determine terrain location</param>
        /// <param name="hitDiggerObjects">Has hit digger objects</param>
        /// <returns></returns>
        public static int GetTextureIndex(RaycastHit hit, out Terrain terrain)
        {
            var chunk = hit.collider.GetComponent<ChunkObject>();
            if (chunk)
            {
                terrain = chunk.Terrain;
                var diggerColliderMesh = chunk.Mesh;
                var triangles = diggerColliderMesh.triangles;
                var triArrayIndex = hit.triangleIndex * 3;

                if (triArrayIndex < 0 || triArrayIndex + 2 >= triangles.Length)
                {
                    Debug.LogWarning(
                        $"Triangle index {hit.triangleIndex} out of range for mesh {diggerColliderMesh.name}.");
                    terrain = null;
                    return -1;
                }

                var baseVertexIndex = triangles[triArrayIndex];


                // return GetMeshTextureIndex(new float4[]
                // {
                //     GetTexcoord(diggerColliderMesh, baseVertexIndex, 1),
                //     GetTexcoord(diggerColliderMesh, baseVertexIndex, 2),
                //     GetTexcoord(diggerColliderMesh, baseVertexIndex, 3),
                //     diggerColliderMesh.tangents[baseVertexIndex]
                // });
                var texcoords = new float4[4];
                var valid =
                    TryGetTexcoord(diggerColliderMesh, baseVertexIndex, 1, out texcoords[0]) &
                    TryGetTexcoord(diggerColliderMesh, baseVertexIndex, 2, out texcoords[1]) &
                    TryGetTexcoord(diggerColliderMesh, baseVertexIndex, 3, out texcoords[2]);

                if (baseVertexIndex < 0 || baseVertexIndex >= diggerColliderMesh.tangents.Length)
                {
                    terrain = null;
                    return -1;
                }

                texcoords[3] = diggerColliderMesh.tangents[baseVertexIndex];

                if (!valid)
                {
                    terrain = null;
                    return -1;
                }
            }

            terrain = hit.collider.GetComponent<Terrain>();
            if (terrain) return GetTerrainTextureIndex(hit.point, terrain);

            // Handle the case when neither a ChunkObject nor a Terrain was hit
            terrain = null; // Set terrain to null for non-terrain objects
            return -1; // Return -1 or another value to indicate "no texture"
        }

        private static bool HasValidUVs(Mesh mesh, int baseVertexIndex, int[] channels)
        {
            foreach (var channel in channels)
            {
                uvs.Clear();
                mesh.GetUVs(channel, uvs);
                if (baseVertexIndex < 0 || baseVertexIndex >= uvs.Count)
                {
                    Debug.LogWarning(
                        $"UV channel {channel} is invalid at index {baseVertexIndex} (UV count: {uvs.Count})");
                    return false;
                }
            }

            return true;
        }

        private static float4 GetTexcoord(Mesh mesh, int baseVertexIndex, int channel)
        {
            uvs.Clear();
            mesh.GetUVs(channel, uvs);

            if (baseVertexIndex < 0 || baseVertexIndex >= uvs.Count)
            {
                Debug.LogWarning($"Invalid UV index: {baseVertexIndex} for channel {channel}. UV count: {uvs.Count}");
                return float4.zero; // or a sensible default
            }

            return uvs[baseVertexIndex];
        }

        /// <summary>Get number of textures added to the terrain</summary>
        /// <param name="worldPos"></param>
        /// <param name="terrain">Terrain to check textures</param>
        /// <returns>Array containing the relative mix of textures on the main terrain at this world position.</returns>
        public static float[] GetTextureMix(Vector3 worldPos, Terrain terrain)
        {
            var terrainData = terrain.terrainData;
            var terrainPos = terrain.transform.position;
            // calculate which splat map cell the worldPos falls within (ignoring y)
            var mapX = (int)((worldPos.x - terrainPos.x) / terrainData.size.x * terrainData.alphamapWidth);
            var mapZ = (int)((worldPos.z - terrainPos.z) / terrainData.size.z * terrainData.alphamapHeight);
            // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            var splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            // extract the 3D array data to a 1D array:
            var cellMix = new float[splatmapData.GetUpperBound(2) + 1];
            for (var n = 0; n < cellMix.Length; n++) cellMix[n] = splatmapData[0, 0, n];
            return cellMix;
        }

        /// <summary> Get most dominant texture given a world position on a terrain </summary>
        /// <param name="worldPos"></param>
        /// <param name="terrain">Terrain to check textures</param>
        /// <returns>Zero-based index of the most dominant texture on the main terrain at this world position.</returns>
        public static int GetTerrainTextureIndex(Vector3 worldPos, Terrain terrain)
        {
            var mix = GetTextureMix(worldPos, terrain);
            float maxMix = 0;
            var maxIndex = 0;
            // loop through each mix value and find the maximum
            for (var n = 0; n < mix.Length; n++)
                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }

            return maxIndex;
        }

        public static int GetMeshTextureIndex(float4[] controls)
        {
            var index = -1;
            var max = -1f;
            for (var dc = 0; dc < controls.Length; dc++)
            {
                var test = controls[dc];
                for (var df = 0; df < 4; df++)
                {
                    if (df >= 4) continue; // Shouldn't happen, but safe guard
                    if (test[df] > max)
                    {
                        max = test[df];
                        index = dc * 4 + df;
                    }
                }
            }

            return index;
        }
    }
}