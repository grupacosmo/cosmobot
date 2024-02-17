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
        private GameData gameData;

        private const string DefaultSaveFileName = "save_file";

        public void OnSceneUnloaded(Scene scene)
        {
            SaveGame();
        }

        public void NewGame()
        {
            this.gameData = new GameData();
        }

        private List<ISaveableData> GetSaveableObjects()
        {
            return FindObjectsOfType<MonoBehaviour>().OfType<ISaveableData>().ToList();
        }

        public void LoadGame(string save_file_name = DefaultSaveFileName)
        {
            SaveFileHandler fileHandler = new SaveFileHandler(save_file_name);
            this.gameData = fileHandler.Load();

            if (this.gameData == null)
            {
                Debug.Log("No game data found, starting new game");
                NewGame();

                return;
            }

            foreach (ISaveableData saveableObject in GetSaveableObjects())
            {
                saveableObject.LoadData(gameData);
            }
            Debug.Log("Game loaded");
        }

        public void SaveGame(string save_file_name = DefaultSaveFileName)
        {
            foreach (ISaveableData saveableObject in GetSaveableObjects())
            {
                if (saveableObject.SaveData(gameData))
                {
                    Debug.Log("Failed to save " + saveableObject);
                }
            }
            Debug.Log("Game saved");
            SaveFileHandler File_handler = new SaveFileHandler(save_file_name);
            File_handler.Save(gameData);
        }

        // this can be removed it this behaviour is not intended
        private void OnApplicationQuit()
        {
            SaveGame();
        }
    }
}
