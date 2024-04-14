using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;

namespace Cosmobot.ItemSystem
{
    public static class CraftingRecipeSerializer
    {
        public static void Serialize(string path, CraftingRecipeSerializationObject recipesObject)
        {
            string json = JsonUtility.ToJson(recipesObject, true);
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Deserializes a CraftingRecipeSerializationObject from a file at the given path. First tries to load the file
        /// as a TextAsset in the editor. If that fails, it will try to read the file from disk. If that fails, it will
        /// return null.
        /// </summary>
        /// <returns>Deserialized object or null if failed</returns>
        [CanBeNull]
        public static CraftingRecipeSerializationObject Deserialize(string path)
        {
            try
            {
                string json = ReadFile(path);
                CraftingRecipeSerializationObject obj = JsonUtility.FromJson<CraftingRecipeSerializationObject>(json);
                if (obj is null)
                {
                    Debug.LogError("Failed to deserialize CraftingRecipeSerializationObject from file at path {path}");    
                }

                return obj;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to read file at path {path}: {e.Message}");
                return null;
            }
        }

        private static string ReadFile(string path)
        {
# if UNITY_EDITOR
            TextAsset asset = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            if (asset) return asset.text;
# endif
            
            return System.IO.File.ReadAllText(path);
        }
    }
}