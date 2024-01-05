using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Cosmobot
{
    public class SaveFileHandler
    {
        private string save_file_path = "";
        private string save_file_name = "";

        public SaveFileHandler(string save_file_name)
        {
            this.save_file_name = save_file_name + ".json";
            this.save_file_path = Path.Combine(Application.persistentDataPath, this.save_file_name);
            Directory.CreateDirectory(Application.persistentDataPath);
        }
        public void Save(GameData game_data)
        {
            // For now we'll just overwrite the file each time
            if (File.Exists(save_file_path))
            {
                Debug.Log("Save file already exists, deleting old one!");
                File.Delete(save_file_path);
            }
            try
            {
                using FileStream stream = File.Create(save_file_path);
                stream.Close();
                File.WriteAllText(save_file_path, JsonConvert.SerializeObject(game_data));
                Debug.Log("Save file path: " + save_file_path);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create save file due to:  + {e.Message} {e.StackTrace}");
            }

        }

        public GameData Load()
        {
            if (File.Exists(save_file_path))
            {
                try
                {
                    string json = File.ReadAllText(save_file_path);
                    return JsonConvert.DeserializeObject<GameData>(json);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to load save file due to:  + {e.Message} {e.StackTrace}");
                }
            }
            return null;
        }
    }
}
