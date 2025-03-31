using Domains.Player.Events;
using Domains.Player.Scripts.ScriptableObjects;
using Domains.UI_Global;
using Domains.UI_Global.Events;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Domains.Player.Scripts
{
#if UNITY_EDITOR
    public static class PlayerStaminaManagerDebug
    {
        [MenuItem("Debug/Reset/Reset Fuel")]
        public static void ResetFuel()
        {
            PlayerFuelManager.ResetPlayerFuel();
        }
    }
#endif
    public class PlayerFuelManager : MonoBehaviour, MMEventListener<FuelEvent>
    {
        public static float FuelPoints;
        public static float MaxFuelPoints;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public static float InitialCharacterFuel;

        [FormerlySerializedAs("staminaBarUpdater")]
        public FuelBarUpdater fuelBarUpdater;


        private string _savePath;
        private CharacterStatProfile characterStatProfile;


        private void Awake()
        {
            if (fuelBarUpdater == null)
            {
                fuelBarUpdater = FindFirstObjectByType<FuelBarUpdater>();
                if (fuelBarUpdater == null)
                    UnityEngine.Debug.LogError("PlayerFuelManager: No FuelBarUpdater found in scene!");
            }

            characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);
            if (characterStatProfile != null)
                InitialCharacterFuel = characterStatProfile.InitialMaxStamina;
            else
                UnityEngine.Debug.LogError("CharacterStatProfile not set in PlayerFuelManager");


            if (FuelPoints == 0) FuelEvent.Trigger(FuelEventType.SetMaxStamina, InitialCharacterFuel * 0.1f);
        }


        private void Start()
        {
            _savePath = GetSaveFilePath();

            if (!ES3.FileExists(_savePath))
            {
                UnityEngine.Debug.Log("[PlayerStaminaManager] No save file found, forcing initial save...");
                ResetPlayerFuel(); // Ensure default values are set
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

        public void OnMMEvent(FuelEvent fuelEvent)
        {
            switch (fuelEvent.EventType)
            {
                case FuelEventType.ConsumeStamina:
                    ConsumeFuel(fuelEvent.ByValue);
                    break;
                case FuelEventType.RecoverStamina:
                    RecoverFuel(fuelEvent.ByValue);
                    break;
                case FuelEventType.FullyRecoverStamina:
                    FullyRecoverFuel();
                    break;
                case FuelEventType.IncreaseMaximumStamina:
                    IncreaseMaximumFuel(fuelEvent.ByValue);
                    break;
                case FuelEventType.SetCurrentStamina:
                    SetCurrentStamina(fuelEvent.ByValue);
                    break;
                case FuelEventType.SetMaxStamina:
                    FuelPoints = fuelEvent.ByValue;
                    break;
            }
        }

        private void SetCurrentStamina(float value)
        {
            FuelPoints = value;
            SavePlayerFuel();
        }

        public void Initialize()
        {
            ResetPlayerFuel();
            fuelBarUpdater.Initialize();
        }

        public static void ConsumeFuel(float amount)
        {
            if (FuelPoints - amount < 0)
            {
                FuelPoints = 0;
                PlayerStatusEvent.Trigger(PlayerStatusEventType.OutOfFuel);
                AlertEvent.Trigger(AlertReason.OutOfFuel, "You are out of fuel!", "Out of Fuel");
            }
            else
            {
                FuelPoints -= amount;
            }
        }

        public static void RecoverFuel(float amount)
        {
            if (FuelPoints == 0 && amount > 0) PlayerStatusEvent.Trigger(PlayerStatusEventType.RegainedStamina);
            FuelPoints += amount;
        }

        public static void FullyRecoverFuel()
        {
            FuelPoints = MaxFuelPoints;
            PlayerStatusEvent.Trigger(PlayerStatusEventType.RegainedStamina);
        }

        public static void IncreaseMaximumFuel(float amount)
        {
            MaxFuelPoints += amount;
        }

        public static void DecreaseMaximumFuel(float amount)
        {
            MaxFuelPoints -= amount;
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
                FuelPoints = ES3.Load<float>("StaminaPoints", saveFilePath);
                MaxFuelPoints = ES3.Load<float>("MaxStaminaPoints", saveFilePath);
                fuelBarUpdater.Initialize();
                UnityEngine.Debug.Log(
                    $"✅ Loaded stamina data: StaminaPoints={FuelPoints}, MaxStaminaPoints={MaxFuelPoints}");
            }
            else
            {
                UnityEngine.Debug.LogError($"❌ No saved stamina data found at {saveFilePath}");
                ResetPlayerFuel();
                fuelBarUpdater.Initialize();
            }
        }

        public static void ResetPlayerFuel()
        {
            var characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            if (characterStatProfile == null)
            {
                UnityEngine.Debug.LogError("\u274c CharacterStatProfile not found! Using default values.");
                FuelPoints = 100f;
                MaxFuelPoints = 100f;
            }
            else
            {
                FuelPoints = characterStatProfile.InitialMaxStamina;
                MaxFuelPoints = characterStatProfile.InitialMaxStamina;
            }

            PlayerStatusEvent.Trigger(PlayerStatusEventType.ResetStamina);
        }

        public static void SavePlayerFuel()
        {
            ES3.Save("StaminaPoints", FuelPoints, "GameSave.es3");
            ES3.Save("MaxStaminaPoints", MaxFuelPoints, "GameSave.es3");
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(GetSaveFilePath());
        }

        public static bool IsPlayerOutOfFuel()
        {
            return FuelPoints <= 0;
        }
    }
}