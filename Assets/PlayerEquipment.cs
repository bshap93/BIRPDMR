using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    public static PlayerEquipment Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject currentToolGraphicalObject;

    public ToolType currentToolType;
    public ToolIteration currentToolIteration;
}