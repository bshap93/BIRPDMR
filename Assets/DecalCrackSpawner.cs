using UnityEngine;

public class DecalCrackSpawner : MonoBehaviour
{
    public DecalType decalType; // insert your Decal Type here

    public int
        maxTrisTotal =
            4096; // maximum triangles this spawner can generate for all decals using a particular lightmap ID or being parented to a particular movable object

    public int maxTrisInDecal = 1024; // maximum triangles per one decal
    public float decalSize = 0.2f; // width and height of the decal
    public float forwardDistance = 1.0f; // forward projection distance
    public float opacity = 1.0f; // opacity of the decal

    private DecalSpawner spawner;

    private void Start()
    {
        spawner = DecalManager.GetSpawner(decalType.decalSettings, maxTrisTotal,
            maxTrisInDecal); // Create Decal Spawner for our Decal Type
    }

    public void ApplyDecal()
    {
        Debug.Log("Applying decal");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit)) // Raycast from our cube
        {
            Transform rootObject = null;
            if (hit.rigidbody != null)
                rootObject =
                    hit.rigidbody.transform; // Consider object movable if it has a rigidbody and get its transform
            spawner.AddDecal(transform.position, transform.rotation, hit.collider.gameObject, decalSize, decalSize,
                forwardDistance, opacity, 0, rootObject); // Spawn the decal
        }
    }

    public void RemoveDecal()
    {
        // DecalManager.Cleanup();
        spawner.Clear();
    }
}