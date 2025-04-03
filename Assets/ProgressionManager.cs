using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public const int NumBeacons = 5;
    public static bool[] BeaconsActivated = new bool[NumBeacons];

    public static bool IsFirstTimeOpeningGameplayScene = true;
}