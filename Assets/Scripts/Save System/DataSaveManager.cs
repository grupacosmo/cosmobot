using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;


namespace Cosmobot
{
    public class DataSaveManager : MonoBehaviour
    {
        private GameData game_data;
        public static DataSaveManager Instance { get; private set; }
        private List<ISaveableData> saveable_objects;

        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning("Multiple instances of DataSaveManager found!");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            this.saveable_objects = FindObjectsOfType<MonoBehaviour>().OfType<ISaveableData>().ToList();
            LoadGame();
        }

        public void NewGame()
        {
            this.game_data = new GameData();
        }

        public void LoadGame()
        {
            SaveFileHandler file_handler = new SaveFileHandler(save_file_name: "save_file");
            this.game_data = file_handler.Load();

            if (this.game_data == null)
            {
                Debug.Log("No game data found, starting new game");
                NewGame();

                return;
            }

            foreach (ISaveableData saveableObject in saveable_objects)
            {
                saveableObject.LoadData(game_data);
            }


        }

        public void SaveGame()
        {
            foreach (ISaveableData saveableObject in saveable_objects)
            {
                if (saveableObject.SaveData(game_data))
                {
                    Debug.Log("Failed to save " + saveableObject);
                }
            }
            Debug.Log("Game saved");
            SaveFileHandler File_handler = new SaveFileHandler(save_file_name: "save_file");
            File_handler.Save(game_data);
        }

        // this can be removed it this behaviour is not intended
        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}