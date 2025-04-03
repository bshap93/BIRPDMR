using Domains.Player.Scripts;
using Domains.Player.Scripts.ScriptableObjects;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public const int NumBeacons = 3;
    public static bool[] BeaconsActivated = new bool[NumBeacons];

    public static bool SkipTutorial = true;


    private CharacterStatProfile characterStatProfile;

    private void Awake()
    {
        characterStatProfile =
            Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

        if (characterStatProfile != null)
            SkipTutorial = characterStatProfile.SkipTutorial;
        else
            Debug.LogError("CharacterStatProfile not set in ProgressionManager");
    }
}