using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Cosmobot.ItemSystem.Editor
{
    [CustomEditor(typeof(ItemInfo))]
    public class ItemInfoInspector : UnityEditor.Editor
    {
        private ItemInfo itemInfo;

        private static string[] declaredConstants;


        private void OnEnable()
        {
            itemInfo = (ItemInfo)target;

            InitializeDeclaredConstants();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();

            if (itemInfo.AdditionalData is null) return;

            string[] undeclaredConstants =
                itemInfo.AdditionalData.Keys
                    .Where(key => !declaredConstants.Contains(key))
                    .ToArray();

            if (undeclaredConstants.Length == 0) return;

            string joined = string.Join(", ", undeclaredConstants);
            EditorGUILayout.HelpBox(
                $"The following keys are not declared in {nameof(ItemDataConstants)}: \n{joined}",
                MessageType.Warning);
        }

        private static void InitializeDeclaredConstants()
        {
            if (declaredConstants is not null) return;

            declaredConstants =
                typeof(ItemDataConstants)
                    .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Select(field => (string)field.GetValue(null))
                    .ToArray();
        }
    }
}