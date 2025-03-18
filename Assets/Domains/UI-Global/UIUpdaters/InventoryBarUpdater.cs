using System;
using Domains.Items.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.UI
{
    public class InventoryBarUpdater : MonoBehaviour, MMEventListener<InventoryEvent>
    {
        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(InventoryEvent eventType)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}