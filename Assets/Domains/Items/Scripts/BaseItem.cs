using UnityEngine;

namespace Domains.Items
{
    [CreateAssetMenu(fileName = "BaseItem", menuName = "Scriptable Objects/BaseItem")]
    public class BaseItem : ScriptableObject
    {
        public string ItemID;


        public string ItemName;

        public string ItemDescription;

        public Sprite ItemIcon;

        public float ItemWeight;

        public int ItemValue;

        public virtual bool Pick()
        {
            return true;
        }


        public static bool IsNull(BaseItem baseItem)
        {
            if (baseItem == null) return true;
            if (baseItem.ItemID == null) return true;
            if (baseItem.ItemID == "") return true;
            return false;
        }
    }
}