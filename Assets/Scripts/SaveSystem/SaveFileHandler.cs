using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Cosmobot
{
    public class SaveFileHandler
    {
        private readonly string saveFilePath;

        public SaveFileHandler(string save_file_name)
        {
            saveFilePath = Path.Combine(
                Application.persistentDataPath,
                save_file_name + ".json"
            );
            Directory.CreateDirectory(Application.persistentDataPath);
        }

        public void Save(GameData game_data)
        {
            // For now we'll just overwrite the file each time
            try
            {
                File.WriteAllText(saveFilePath, JsonConvert.SerializeObject(game_data));
                Debug.Log("Save file path: " + saveFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create save file due to:  + {e.Message} {e.StackTrace}");
            }
        }

        public GameData Load()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    return JsonConvert.DeserializeObject<GameData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"Failed to load save file due to:  + {e.Message} {e.StackTrace}"
                    );
                }
            }

            return null;
        }
    }
}
