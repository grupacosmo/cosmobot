using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Cosmobot
{
    public class DataSaveManager : MonoBehaviour
    {
        private GameData gameData;
        public static DataSaveManager Instance { get; private set; }
        private List<ISaveableData> saveableObjects;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            saveableObjects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveableData>().ToList();
            LoadGame();
        }
        public void NewGame()
        {
            this.gameData = new GameData();
        }

        public void LoadGame()
        {
            if (this.gameData == null)
            {
                Debug.Log("No game data found, starting new game");
                NewGame();
                return;
            }

            foreach (ISaveableData saveableObject in saveableObjects)
            {
                saveableObject.LoadData(gameData);
            }
        }

        public void SaveGame()
        {
            foreach (ISaveableData saveableObject in saveableObjects)
            {
                if (saveableObject.SaveData(gameData))
                {
                    Debug.Log("Failed to save " + saveableObject);
                }
            }
            Debug.Log("Game saved");
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}