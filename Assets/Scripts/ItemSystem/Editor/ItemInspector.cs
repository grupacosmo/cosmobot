using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cosmobot.ItemSystem.Editor
{
    [CustomEditor(typeof(Item))]
    public class ItemInspector : UnityEditor.Editor
    {
        private Item item;

        private static string[] declaredConstants;

        private void OnEnable()
        {
            item = (Item)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (item.ItemInfo is null)
            {
                EditorGUILayout.HelpBox("ItemInfo is not set.", MessageType.Warning);
                return;
            }

            if (item.ItemData is null || item.ItemData.Count == 0)
            {
                if (GUILayout.Button("Add ItemInfo keys"))
                {
                    fillMissingKeys();
                }

                return;
            }

            List<string> infoKeys = item.ItemInfo.AdditionalData.Keys.ToList();
            List<string> itemKeys = item.ItemData.Keys.ToList();

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
                    fillMissingKeys();
                }
            }
        }

        private void fillMissingKeys()
        {
            SerializedProperty keys = serializedObject.FindProperty("ItemData.keys");
            SerializedProperty values = serializedObject.FindProperty("ItemData.values");

            foreach (var additionalField in item.ItemInfo.AdditionalData)
            {
                if (!item.ItemData.ContainsKey(additionalField.Key))
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