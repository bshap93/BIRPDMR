using System;
using System.Collections.Generic;
using Domains.Player.Events;
using Domains.Player.Scripts.ScriptableObjects;
using Domains.Scene.Scripts;
using MoreMountains.Tools;
using UnityEngine;

namespace Domains.Player.Scripts
{
    public class ProgressionManager : MonoBehaviour, MMEventListener<ProgressionEvent>
    {
        public const int NumBeacons = 3;
        public static bool[] BeaconsActivated = new bool[NumBeacons];

        public static HashSet<string> UsedBeacons = new();

        public static bool SkipTutorial = true;

        private string _savePath;


        private CharacterStatProfile characterStatProfile;


        private void Awake()
        {
            characterStatProfile =
                Resources.Load<CharacterStatProfile>(CharacterResourcePaths.CharacterStatProfileFilePath);

            if (characterStatProfile != null)
                SkipTutorial = characterStatProfile.SkipTutorial;
            else
                UnityEngine.Debug.LogError("CharacterStatProfile not set in ProgressionManager");
        }

        private void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                UnityEngine.Debug.Log("[ProgressionManager] No save file found, forcing initial save...");
                ResetProgression(); // Ensure default values are set
            }

            LoadProgression();
        }

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ProgressionEvent eventType)
        {
        }

        public static void AddUsedBeacon(string uniqueID, bool b)
        {
            if (b)
                UsedBeacons.Add(uniqueID);

            UnityEngine.Debug.Log($"Beacon {uniqueID} used: {b}");
        }

        public static void ResetProgression()
        {
        }

        public bool HasSavedData()
        {
            return false;
        }

        public static void SaveAllProgression()
        {
            throw new NotImplementedException();
        }

        public static void LoadProgression()
        {
        }

        private static string GetSaveFilePath()
        {
            return SaveManager.SaveProgressionFilePath;
        }
    }
}