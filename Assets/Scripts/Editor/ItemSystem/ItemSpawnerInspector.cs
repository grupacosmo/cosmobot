using System.Collections.Generic;
using System.Linq;
using Cosmobot.ItemSystem;
using UnityEditor;
using UnityEngine;

namespace Cosmobot.Editor
{
    [CustomEditor(typeof(ItemSpawner))]
    public class ItemSpawnerInspector : UnityEditor.Editor
    {
        private static string[] declaredConstants;
        private ItemSpawner itemSpawner;

        private void OnEnable()
        {
            itemSpawner = (ItemSpawner)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (itemSpawner.ItemInfo is null)
            {
                EditorGUILayout.HelpBox("ItemInfo is not set.", MessageType.Warning);
                return;
            }

            if (itemSpawner.ItemData is null || itemSpawner.ItemData.Count == 0)
            {
                if (GUILayout.Button("Add ItemInfo keys"))
                {
                    AddMissingKeys();
                }

                return;
            }

            List<string> infoKeys = itemSpawner.ItemInfo.AdditionalData.Keys.ToList();
            List<string> itemKeys = itemSpawner.ItemData.Keys.ToList();

            IEnumerable<string> missingKeys = infoKeys.Except(itemKeys);
            IEnumerable<string> extraKeys = itemKeys.Except(infoKeys);

            string missingKeysString = string.Join(", ", missingKeys);
            if (missingKeysString.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    "ItemInfo defines additional data keys that are not present here: " + missingKeysString,
                    MessageType.Info);
            }

            string extraKeysString = string.Join(", ", extraKeys);
            if (extraKeysString.Length > 0)
            {
                EditorGUILayout.HelpBox("ItemData contains keys that are not defined in ItemInfo: " + extraKeysString,
                    MessageType.Warning);
            }

            if (missingKeysString.Length > 0)
            {
                if (GUILayout.Button("Add missing ItemInfo keys"))
                {
                    AddMissingKeys();
                }
            }
        }

        private void AddMissingKeys()
        {
            SerializedProperty keys = serializedObject.FindProperty("itemData.keys");
            SerializedProperty values = serializedObject.FindProperty("itemData.values");

            foreach (var additionalField in itemSpawner.ItemInfo.AdditionalData)
            {
                if (!itemSpawner.ItemData.ContainsKey(additionalField.Key))
                {
                    keys.InsertArrayElementAtIndex(keys.arraySize);
                    values.InsertArrayElementAtIndex(values.arraySize);
                    keys.GetArrayElementAtIndex(keys.arraySize - 1).stringValue = additionalField.Key;
                    values.GetArrayElementAtIndex(values.arraySize - 1).stringValue = additionalField.Value;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
