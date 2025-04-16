using Domains.UI_Global.Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.UI_Global.Scripts
{
    public class PlayerUIManager : MonoBehaviour
    {
        public static PlayerUIManager Instance;

        [FormerlySerializedAs("SkipIntroduction")]
        public bool skipIntroduction;

        public PlayerUIManager(bool isUIOpen)
        {
            IsUIOpen = isUIOpen;
        }

        public bool IsUIOpen { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (skipIntroduction)
            {
                UIEvent.Trigger(UIEventType.CloseUI);
                IsUIOpen = false;
            }
            else
            {
                IsUIOpen = true;
            }
        }
    }
}