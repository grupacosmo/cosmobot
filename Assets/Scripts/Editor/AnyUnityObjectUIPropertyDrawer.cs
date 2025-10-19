using System;
using Cosmobot.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cosmobot.Editor
{
    [CustomPropertyDrawer(typeof(AnyUnityObjectUIAttribute))]
    public class AnyUnityObjectUIPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            object obj = property.managedReferenceValue;
            Object unityObj = obj as Object;

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            Object assigned = EditorGUI.ObjectField(position, unityObj, typeof(Object), true);
            EditorGUI.EndProperty();

            if (ReferenceEquals(obj, assigned)) return;

            if (assigned == null)
            {
                property.managedReferenceValue = null;
                return;
            }

            // [assembly] [namespace].[type]
            string[] fullyQualifiedType = property.managedReferenceFieldTypename.Split(" ");
            string propertyTypeName = fullyQualifiedType.Length > 1 ? fullyQualifiedType[1] : fullyQualifiedType[0];
            // string propertyTypeName = property.type;

            Type propertyType = Type.GetType(propertyTypeName);
            if (propertyType is null)
            {
                Debug.LogError($"Could not find C# type ('{propertyTypeName}') for property {property.name}");
                return;
            }

            if (!propertyType.IsInstanceOfType(assigned))
            {
                Debug.LogWarning($"Assigned object ('{assigned.name}' of type '{assigned.GetType()}') "
                                 + $"is not subclass of '{propertyType}'");
                return;
            }

            property.managedReferenceValue = assigned;
        }
    }
}
