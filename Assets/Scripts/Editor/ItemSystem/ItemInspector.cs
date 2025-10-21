using System.Collections.Generic;
using System.Linq;
using Cosmobot.ItemSystem;
using UnityEditor;
using UnityEngine;

namespace Cosmobot.Editor.ItemSystem
{
    [CustomEditor(typeof(ItemComponent))]
    public class ItemInspector : UnityEditor.Editor
    {
        private static string[] declaredConstants;
        private ItemComponent itemComponent;

        private void OnEnable()
        {
            itemComponent = (ItemComponent)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (itemComponent.ItemInfo is null)
            {
                EditorGUILayout.HelpBox("ItemInfo is not set.", MessageType.Warning);
                return;
            }

            if (itemComponent.ItemData is null || itemComponent.ItemData.Count == 0)
            {
                if (GUILayout.Button("Add ItemInfo keys"))
                {
                    AddMissingKeys();
                }

                return;
            }

            List<string> infoKeys = itemComponent.ItemInfo.AdditionalData.Keys.ToList();
            List<string> itemKeys = itemComponent.ItemData.Keys.ToList();

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
            SerializedProperty keys = serializedObject.FindProperty("item.itemData.keys");
            SerializedProperty values = serializedObject.FindProperty("item.itemData.values");

            foreach (var additionalField in itemComponent.ItemInfo.AdditionalData)
            {
                if (!itemComponent.ItemData.ContainsKey(additionalField.Key))
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
