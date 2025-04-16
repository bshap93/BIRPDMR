using System.Collections.Generic;
using Domains.Items.Events;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Domains.Gameplay.Vendor.Train
{
    [ShowOdinSerializedPropertiesInInspector]
    public class ModularTrainController : MonoBehaviour,
        ISerializationCallbackReceiver, MMEventListener<InventoryEvent>
    {
        [SerializeField] [HideInInspector] private SerializationData serializationData;

        [OdinSerialize] private Queue<TrainSegmentController> trainSegments = new();

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            UnitySerializationUtility.DeserializeUnityObject(this, ref serializationData);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UnitySerializationUtility.SerializeUnityObject(this, ref serializationData);
        }

        public void OnMMEvent(InventoryEvent eventType)
        {
            if (eventType.EventType == InventoryEventType.SellAllItems) SendOffHeadOfTrainQueue();
        }

        public void SendOffHeadOfTrainQueue()
        {
            if (trainSegments.Count > 0)
            {
                var trainSegment = trainSegments.Dequeue();
                trainSegment.SendOff();
            }
        }
    }
}