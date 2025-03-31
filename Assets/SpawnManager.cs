using System.Collections;
using Domains.Player.Scripts;
using Lightbug.CharacterControllerPro.Core;
using MoreMountains.Tools;
using UnityEngine;

public class SpawnManager : MonoBehaviour, MMEventListener<PlayerStatusEvent>
{
    public static SpawnManager Instance;
    public GameObject player01;
    private TeleportPlayer _player01TeleportPlayer;
    private CharacterActor characterActor;

    private void Awake()
    {
        Instance = this;
        _player01TeleportPlayer = GetComponent<TeleportPlayer>();

        characterActor = player01.GetComponent<CharacterActor>();

        if (characterActor == null)
            Debug.LogError("CharacterActor component not found on player01.");

        if (_player01TeleportPlayer == null)
            Debug.LogError("PositionAndRotationModifier component not found on SpawnManager.");
    }

    private void OnEnable()
    {
        this.MMEventStartListening();
    }

    private void OnDisable()
    {
        this.MMEventStopListening();
    }

    public void OnMMEvent(PlayerStatusEvent eventType)
    {
        if (eventType.EventType == PlayerStatusEventType.Died) TeleportPlayerToSpawn();

        if (eventType.EventType == PlayerStatusEventType.OutOfFuel)
        {
            TeleportPlayerToSpawn();
            // Add a slight delay to ensure death manager has processed first
            StartCoroutine(VerifyFuelAfterTeleport());
        }
    }

    private IEnumerator VerifyFuelAfterTeleport()
    {
        yield return new WaitForSeconds(0.2f);

        // Double-check that player has enough fuel to move
        if (PlayerFuelManager.FuelPoints < 10f)
        {
            Debug.LogWarning("Player fuel still too low after teleport, forcing minimum");
            PlayerFuelManager.EnsureMinimumFuel();
        }
    }

// In SpawnManager.cs
    private void TeleportPlayerToSpawn()
    {
        _player01TeleportPlayer.Teleport(characterActor);

        // Debug when teleporting
        Debug.Log("Player teleported to spawn point");
    }
}