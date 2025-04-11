using System.Linq;
using Digger.Modules.Runtime.Sources;
using Domains.Gameplay.Mining.Scripts;
using Domains.Player.Camera;
using Domains.Player.Events;
using Domains.Player.Scripts;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Gameplay.Tools.ToolSpecifics
{
    public class ShovelTool : BaseDiggerUsingTool, MMEventListener<UpgradeEvent>
    {
        private void Awake()
        {
            digger = FindFirstObjectByType<DiggerMasterRuntime>();
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }


        public void OnMMEvent(UpgradeEvent eventType)
        {
            if (eventType.EventType == UpgradeEventType.ShovelMiningSizeSet)
                SetDiggerUsingToolEffectSize(eventType.EffectValue, eventType.EffectValue2);
        }

        public override void UseTool(RaycastHit hit)
        {
            lastHit = hit;
            PerformToolAction();
        }


        public override void PerformToolAction()
        {
            var textureIndex = GetCurrentTextureIndex();


            if (Time.time < lastDigTime + miningCooldown)
                return;

            lastDigTime = Time.time;

            if (playerInteraction == null || digger == null)
                return;


            var notPlayerMask = ~playerInteraction.playerLayerMask;
            if (!Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out var hit,
                    diggerUsingRange,
                    notPlayerMask))
                return;

            // Cache hit for external access
            lastHit = hit;

            // Interact
            if (CanInteractWithObject(hit.collider.gameObject))
            {
                // Call IInteractable if implemented
                hit.collider.GetComponent<IInteractable>()?.Interact();


                var minable = hit.collider.GetComponent<IMinable>();
                if (minable != null)
                {
                    minable.MinableFailHit(hit.point);
                    moveToolDespiteFailHitFeedbacks?.PlayFeedbacks();
                    CameraEffectEvent.Trigger(CameraEffectEventType.ShakeCameraPosition, 0.2f);
                    return;
                }
            }

            // Return after triggering failed mining feedbacks, and before digging
            if (!allowedTerrainTextureIndices.Contains(textureIndex)) return;


            // Debris FX
            if (debrisEffectPrefab)
            {
                var pos = hit.point + hit.normal * 0.1f;
                var rot = Quaternion.LookRotation(-mainCamera.transform.forward);
                var fx = Instantiate(debrisEffectPrefab, pos, rot);
                Destroy(fx, 2f);
            }

            // Feedback trigger (from PerformToolAction, not MMFeedbacks directly)
            if (diggingFeedbacks != null) diggingFeedbacks.PlayFeedbacks(hit.point);

            // Dig!
            var digPosition = hit.point + mainCamera.transform.forward * 0.3f;

            if (editAsynchronously)
                digger.ModifyAsyncBuffured(digPosition, brush, action, textureIndex, effectOpacity, effectRadius,
                    stalagmiteHeight);
            else
                digger.Modify(digPosition, brush, action, textureIndex, effectOpacity, effectRadius);

            FuelEvent.Trigger(FuelEventType.ConsumeFuel, 2f, PlayerFuelManager.MaxFuelPoints);
        }
    }
}