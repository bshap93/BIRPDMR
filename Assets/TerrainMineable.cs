using Domains.Gameplay.Mining.Scripts;
using Domains.Scripts_that_Need_Sorting;
using MoreMountains.Feedbacks;
using UnityEngine;

public class TerrainMineable : MonoBehaviour, IMinable
{
    [SerializeField] private TextureDetector textureDetector;
    [SerializeField] private MMFeedbacks failHitFeedbacks;
    [SerializeField] private GameObject failHitParticles;

    public void MinableMineHit()
    {
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
}