using Domains.Player.Scripts.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerInfoSheet : MonoBehaviour
{
    public static int WeightLimit;

    [FormerlySerializedAs("InitialStats")] public CharacterStatProfile initialStats;
    public static PlayerInfoSheet Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (initialStats != null)
            WeightLimit = initialStats.InitialWeightLimit;
        else
            Debug.LogError("CharacterStatProfile not set in PlayerInfoSheet");
    }
}