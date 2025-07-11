using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cosmobot
{
    public class DataSaveManager : SingletonSystem<DataSaveManager>
    {
        private const string DefaultSaveFileName = "save_file";
        private GameData gameData;

        // this can be removed it this behaviour is not intended
        private void OnApplicationQuit()
        {
            SaveGame();
        }

        public void OnSceneUnloaded(Scene scene)
        {
            SaveGame();
        }

        public void NewGame()
        {
            gameData = new GameData();
        }

        private List<ISaveableData> GetSaveableObjects()
        {
            return FindObjectsOfType<MonoBehaviour>().OfType<ISaveableData>().ToList();
        }

        public void LoadGame(string save_file_name = DefaultSaveFileName)
        {
            SaveFileHandler fileHandler = new SaveFileHandler(save_file_name);
            gameData = fileHandler.Load();

            if (gameData == null)
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
            SaveFileHandler file_handler = new SaveFileHandler(save_file_name);
            file_handler.Save(gameData);
        }
    }
}
