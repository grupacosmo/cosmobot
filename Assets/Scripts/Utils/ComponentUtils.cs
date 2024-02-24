using UnityEngine;

namespace Cosmobot
{
    public static class ComponentUtils
    {
        /// <summary>
        /// Will log an error message and return false if obj is null
        /// </summary>
        /// <param name="obj">object to check for null</param>
        /// <param name="message">error message to log if obj is null</param>
        /// <param name="context">Unity object that will be used as the context for the error</param>
        /// <returns>True if obj is not null, false otherwise.</returns>
        public static bool RequireNotNull(object obj, string message, Object context)
        {
            if (obj is null)
            {
                Debug.LogError(message, context);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Requires all objects in obj list to be not null. If any of the objects is null, it will 
        /// log an error message and return false.
        /// </summary>
        /// <param name="obj">list of objects, possible nulls. List itself cannot be null.</param>
        /// <param name="fieldNames">list of objects names (null values not allowed). Should be the 
        /// same length as objs. List itself cannot be null.</param>
        /// <param name="context">Unity object that will be used as the context for the error</param>
        /// <returns>True if all objects are not null, false otherwise.</returns>
        public static bool RequireNotNull(object[] objs, string[] fieldNames, Object context) {
            bool anyNull = false;
            for (int i = 0; i < objs.Length; i++) {
                if (objs[i] is null) {
                    Debug.LogError($"{context.name}: {fieldNames[i]} is not set.", context);
                    anyNull = true;
                }
            }
            return !anyNull;
        }
    }
}