using Domains.Player.Events;
using Domains.Player.Scripts.ScriptableObjects;
using Domains.UI;
using Domains.UI_Global;
using Gameplay.Character.Stamina;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace Domains.Player.Scripts
{
#if UNITY_EDITOR
    public static class PlayerStaminaManagerDebug
    {
        [MenuItem("Debug/Reset/Reset Stamina")]
        public static void ResetStamina()
        {
            PlayerStaminaManager.ResetPlayerStamina();
        }
    }
#endif
    public class PlayerStaminaManager : MonoBehaviour, MMEventListener<StaminaEvent>
    {
        public static float StaminaPoints;
        public static float MaxStaminaPoints;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static float InitialCharacterStamina;
        public StaminaBarUpdater staminaBarUpdater;
        public CharacterStatProfile characterStatProfile;


        private string _savePath;


        private void Awake()
        {
            if (characterStatProfile != null)
                InitialCharacterStamina = characterStatProfile.InitialMaxStamina;
            else
                UnityEngine.Debug.LogError("CharacterStatProfile not set in PlayerStaminaManager");
        }


        private void Start()
        {
            _savePath = GetSaveFilePath();

            if (!ES3.FileExists(_savePath))
            {
                UnityEngine.Debug.Log("[PlayerStaminaManager] No save file found, forcing initial save...");
                ResetPlayerStamina(); // Ensure default values are set
            }

            LoadPlayerStamina();
        }


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(StaminaEvent staminaEvent)
        {
            switch (staminaEvent.EventType)
            {
                case StaminaEventType.ConsumeStamina:
                    ConsumeStamina(staminaEvent.ByValue);
                    break;
                case StaminaEventType.RecoverStamina:
                    RecoverStamina(staminaEvent.ByValue);
                    break;
                case StaminaEventType.FullyRecoverStamina:
                    FullyRecoverStamina();
                    break;
                case StaminaEventType.IncreaseMaximumStamina:
                    IncreaseMaximumStamina(staminaEvent.ByValue);
                    break;
                case StaminaEventType.SetMaxStamina:
                    StaminaPoints = staminaEvent.ByValue;
                    break;
            }
        }

        public void Initialize()
        {
            ResetPlayerStamina();
            staminaBarUpdater.Initialize();
        }

        public static void ConsumeStamina(float amount)
        {
            if (StaminaPoints - amount < 0)
            {
                StaminaPoints = 0;
                PlayerStatusEvent.Trigger(PlayerStatusEventType.OutOfStamina);
                AlertManager.ShowAlert("Stamina Depleted");
            }
            else
            {
                StaminaPoints -= amount;
            }
        }

        public static void RecoverStamina(float amount)
        {
            if (StaminaPoints == 0 && amount > 0) PlayerStatusEvent.Trigger(PlayerStatusEventType.RegainedStamina);
            StaminaPoints += amount;
        }

        public static void FullyRecoverStamina()
        {
            StaminaPoints = MaxStaminaPoints;
            PlayerStatusEvent.Trigger(PlayerStatusEventType.RegainedStamina);
        }

        public static void IncreaseMaximumStamina(float amount)
        {
            MaxStaminaPoints += amount;
        }

        public static void DecreaseMaximumStamina(float amount)
        {
            MaxStaminaPoints -= amount;
        }

        private static string GetSaveFilePath()
        {
            return "GameSave.es3"; // Always use the same file
        }

        public void LoadPlayerStamina()
        {
            var saveFilePath = GetSaveFilePath();

            if (ES3.FileExists(saveFilePath))
            {
                StaminaPoints = ES3.Load<float>("StaminaPoints", saveFilePath);
                MaxStaminaPoints = ES3.Load<float>("MaxStaminaPoints", saveFilePath);
                staminaBarUpdater.Initialize();
                UnityEngine.Debug.Log(
                    $"✅ Loaded stamina data: StaminaPoints={StaminaPoints}, MaxStaminaPoints={MaxStaminaPoints}");
            }
            else
            {
                UnityEngine.Debug.LogError($"❌ No saved stamina data found at {saveFilePath}");
                ResetPlayerStamina();
                staminaBarUpdater.Initialize();
            }
        }

        public static void ResetPlayerStamina()
        {
            var characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            if (characterStatProfile == null)
            {
                UnityEngine.Debug.LogError("\u274c CharacterStatProfile not found! Using default values.");
                StaminaPoints = 100f;
                MaxStaminaPoints = 100f;
            }
            else
            {
                StaminaPoints = characterStatProfile.InitialMaxStamina;
                MaxStaminaPoints = characterStatProfile.InitialMaxStamina;
            }

            PlayerStatusEvent.Trigger(PlayerStatusEventType.ResetStamina);
        }

        public static void SavePlayerStamina()
        {
            ES3.Save("StaminaPoints", StaminaPoints, "GameSave.es3");
            ES3.Save("MaxStaminaPoints", MaxStaminaPoints, "GameSave.es3");
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(GetSaveFilePath());
        }

        public static bool IsPlayerOutOfStamina()
        {
            return StaminaPoints <= 0;
        }
    }
}