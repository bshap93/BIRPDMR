using Digger.Demo;
using TMPro;
using UnityEngine;

public class TerrainDetectorUI : MonoBehaviour
{
    public TextureDetector terrainDetector;
    [SerializeField] private TMP_Text text;

    // Update is called once per frame
    private void Update()
    {
        // every second, update the text
        if (terrainDetector.texture == null) return;
        if (Time.frameCount % 60 == 0) text.text = $"Terrain: {terrainDetector.texture}";
    }
}