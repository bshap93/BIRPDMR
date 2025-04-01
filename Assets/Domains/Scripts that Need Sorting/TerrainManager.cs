using Domains.Scripts_that_Need_Sorting;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static int CurrentTextureIndex;
    private TextureDetector textureDetector;

    private void Awake()
    {
        textureDetector = FindFirstObjectByType<TextureDetector>();
        CurrentTextureIndex = -1;
    }
}