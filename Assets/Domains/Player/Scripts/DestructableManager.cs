using System;
using System.Collections.Generic;
using Domains.Player.Events;
using Domains.Scene.Scripts;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace Domains.Player.Scripts
{
#if UNITY_EDITOR

    public static class DestructableManagerDebug
    {
        [MenuItem("Debug/Reset Destructables")]
        public static void ResetDestructables()
        {
            DestructableManager.ResetDestructables();
        }
    }
#endif
    public class DestructableManager : MonoBehaviour, MMEventListener<DestructableEvent>
    {
        public static HashSet<string> DestroyedOreNodes = new();

        private Dictionary<string, bool> _destroyedOreNodeWasDestroyed;

        private string _savePath;

        private void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                UnityEngine.Debug.Log("[DestructableManager] No save file found, forcing initial save...");
                ResetDestructables(); // Ensure default values are set
            }

            LoadDestructables();
        }


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(DestructableEvent eventType)
        {
            throw new NotImplementedException();
        }

        private void LoadDestructables()
        {
            if (_savePath == null) _savePath = GetSaveFilePath();

            if (ES3.FileExists(_savePath))
            {
                if (ES3.KeyExists("Destructables", _savePath))
                {
                    var loadedDestructables = ES3.Load<HashSet<string>>("Destructables", _savePath);
                    DestroyedOreNodes.Clear();

                    foreach (var destructable in loadedDestructables)
                    {
                        DestroyedOreNodes.Add(destructable);
                        UnityEngine.Debug.Log($"Loaded destructable: {destructable}");
                    }
                }
                else
                {
                    var keys = ES3.GetKeys(_savePath);
                    foreach (var key in keys)
                        if (ES3.KeyExists(key, _savePath) && ES3.Load<bool>(key, _savePath))
                        {
                            DestroyedOreNodes.Add(key);
                            UnityEngine.Debug.Log($"Loaded destructable: {key}");
                        }
                }
            }
        }

        public static void SaveAllDestructables()
        {
            var saveFilePath = GetSaveFilePath();

            ES3.Save("Destructables", DestroyedOreNodes, saveFilePath);

            foreach (var uniqueId in DestroyedOreNodes) ES3.Save(uniqueId, true, saveFilePath);
        }

        public static void ResetDestructables()
        {
            DestroyedOreNodes = new HashSet<string>();
        }

        private bool HasSavedData()
        {
            return ES3.FileExists(_savePath);
        }

        private static string GetSaveFilePath()
        {
            return SaveManager.SavePickablesFileName;
        }
    }
}