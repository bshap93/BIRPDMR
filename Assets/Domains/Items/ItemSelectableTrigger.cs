using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Domains.Items
{
    public class ItemSelectableTrigger : MonoBehaviour
    {
        public BaseItem Item;

        [SerializeField] MMFeedbacks selectionFeedbacks;

        public bool NotPickable;
        ItemPicker _itemPicker;

        // PlayerItemListPreviewManager _playerPreviewManager;

        void Awake()
        {
            _itemPicker = GetComponent<ItemPicker>();

            if (_itemPicker == null)
                _itemPicker = gameObject.AddComponent<ItemPicker>();

            // TODO: Maybe this isn't working
            _itemPicker.itemType = Item;
        }

        void OnTriggerEnter(Collider other)
        {
            throw new NotImplementedException();
        }
    }
}
