using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmobot
{
    public class DataSaveManager : SingletonSystem<DataSaveManager>
    {
        private GameData game_data;

        [SerializeField]
        private const string default_save_file_name = "save_file";
        private List<ISaveableData> saveable_objects;

        public void OnSceneUnloaded(Scene scene)
        {
            SaveGame();
        }

        public void NewGame()
        {
            this.game_data = new GameData();
        }

        private void GetSaveableObjects()
        {
            this.saveable_objects = FindObjectsOfType<MonoBehaviour>()
                .OfType<ISaveableData>()
                .ToList();
        }

        public void LoadGame(string save_file_name = default_save_file_name)
        {
            SaveFileHandler file_handler = new SaveFileHandler(save_file_name);
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
            Debug.Log("Game loaded");
        }

        public void SaveGame(string save_file_name = default_save_file_name)
        {
            GetSaveableObjects();

            foreach (ISaveableData saveableObject in saveable_objects)
            {
                if (saveableObject.SaveData(game_data))
                {
                    Debug.Log("Failed to save " + saveableObject);
                }
            }
            Debug.Log("Game saved");
            SaveFileHandler File_handler = new SaveFileHandler(save_file_name);
            File_handler.Save(game_data);
        }

        // this can be removed it this behaviour is not intended
        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}
