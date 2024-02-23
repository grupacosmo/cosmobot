using UnityEngine;

namespace Cosmobot
{
    public static class ComponentUtils
    {
        public static bool RequireNotNull(object obj, string message, Object context)
        {
            if (obj is null)
            {
                Debug.LogError(message, context);
                return false;
            }
            return true;
        }

        public static bool RequireNotNull(object[] obj, string[] fieldNames, Object context) {
            bool anyNull = false;
            for (int i = 0; i < obj.Length; i++) {
                if (obj[i] is null) {
                    Debug.LogError($"{context.name}: {fieldNames[i]} is not set.", context);
                    anyNull = true;
                }
            }
            return !anyNull;
        }
    }
}