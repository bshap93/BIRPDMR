using System;
using Domains.Player.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Gameplay.Tools.ToolSpecifics
{
    public class PickaxeTool : BaseDiggerUsingTool, MMEventListener<UpgradeEvent>
    {
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
            if (eventType.EventType == UpgradeEventType.PickaxeMiningSizeSet)
                SetDiggerUsingToolEffectSize(eventType.EffectValue, eventType.EffectValue2);
        }

        public override void UseTool(RaycastHit hit)
        {
            lastHit = hit;
            PerformToolAction();
        }

        public override void PerformToolAction()
        {
        }

        public override bool CanInteractWithTextureIndex(int index)
        {
            throw new NotImplementedException();
        }

        public override bool CanInteractWithObject(GameObject target)
        {
            throw new NotImplementedException();
        }

        public override void SetDiggerUsingToolEffectSize(float newEffectRadius, float newEffectOpacity)
        {
            throw new NotImplementedException();
        }
    }
}