using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance;

    public PlayerUIManager(bool isUIOpen)
    {
        this.IsUIOpen = isUIOpen;
    }

    public bool IsUIOpen { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IsUIOpen = false;
    }
}