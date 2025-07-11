using UnityEditor;
using UnityEngine;

namespace Cosmobot.Editor
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        private const string PropKeys = "keys";
        private const string PropValues = "values";

        private const float HPadding = 4;
        private const float VPadding = 2;

        private float FieldHeight => EditorGUIUtility.singleLineHeight;
        private float LineHeight => FieldHeight + VPadding;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = position;
            foldoutRect.height = FieldHeight;
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            if (!property.isExpanded) return;

            position.yMin += LineHeight;
            DrawListGUI(position, property);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return CalculatePropertyHeight(property);
        }

        private void DrawListGUI(Rect listRect, SerializedProperty property)
        {
            SerializedProperty keys = property.FindPropertyRelative(PropKeys);
            SerializedProperty values = property.FindPropertyRelative(PropValues);
            EnsureSameSize(keys, values);

            float deleteButtonWidth = 20;
            float halfWidth = (listRect.width - deleteButtonWidth - HPadding) / 2;
            float keyWidth = halfWidth - HPadding * 2;
            float valueWidth = halfWidth - HPadding * 2;
            ;

            GUI.Box(listRect, GUIContent.none);
            listRect.y += LineHeight / 2;

            Rect keyRect = new(
                listRect.x + HPadding,
                listRect.y,
                keyWidth,
                FieldHeight);

            Rect valueRect = new(
                keyRect.xMax + HPadding * 2,
                listRect.y,
                valueWidth,
                FieldHeight);

            Rect deleteButtonRect = new(
                valueRect.xMax + HPadding * 2,
                listRect.y,
                deleteButtonWidth,
                FieldHeight);

            EditorGUI.LabelField(keyRect, "Key");
            EditorGUI.LabelField(valueRect, "Value");

            keyRect.y += LineHeight;
            valueRect.y += LineHeight;
            deleteButtonRect.y += LineHeight;

            for (int i = 0; i < keys.arraySize; i++)
            {
                EditorGUI.PropertyField(keyRect, keys.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUI.PropertyField(valueRect, values.GetArrayElementAtIndex(i), GUIContent.none);
                if (GUI.Button(deleteButtonRect, "-"))
                {
                    keys.DeleteArrayElementAtIndex(i);
                    values.DeleteArrayElementAtIndex(i);
                }

                keyRect.y += LineHeight;
                valueRect.y += LineHeight;
                deleteButtonRect.y += LineHeight;
            }

            if (GUI.Button(valueRect, "+"))
            {
                keys.arraySize++;
                values.arraySize++;
            }
        }

        private static void EnsureSameSize(SerializedProperty keys, SerializedProperty values)
        {
            if (keys.arraySize == values.arraySize) return;

            int size = Mathf.Max(keys.arraySize, values.arraySize);
            keys.arraySize = size;
            values.arraySize = size;
        }

        private float CalculatePropertyHeight(SerializedProperty property)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

            SerializedProperty keys = property.FindPropertyRelative(PropKeys);

            const int AdditionalLines = 4; // label, header row, button, top-down margin
            float fullLineHeight = LineHeight;
            return fullLineHeight * (keys.arraySize + AdditionalLines);
        }
    }
}
