using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    public class Item : MonoBehaviour
    {
        [SerializeField]
        private ItemInfo itemInfo;

        [SerializeField]
        public SerializableDictionary<string, string> ItemData;

        public ItemInfo ItemInfo => itemInfo;

        /// <summary>
        /// Returns the value of the given key, or null if the key does not exist.
        /// </summary>
        public string GetValue(string key)
        {
            return ItemData.GetValueOrDefault(key, null);
        }

        public int? GetIntValue(string key)
        {
            string value = GetValue(key);
            if (value is not null && int.TryParse(value, out int result))
            {
                return result;
            }
            return null;
        }

        public float? GetFloatValue(string key)
        {
            string value = GetValue(key);
            if (value is not null && float.TryParse(value, out float result))
            {
                return result;
            }
            return null;
        }

        public bool? GetBoolValue(string key)
        {
            string value = GetValue(key);
            if (value is not null && bool.TryParse(value, out bool result))
            {
                return result;
            }
            return null;
        }

        public void SetValue(string key, string value)
        {
            ItemData[key] = value;
        }

        public void SetIntValue(string key, int value)
        {
            SetValue(key, value.ToString());
        }

        public void SetFloatValue(string key, float value)
        {
            SetValue(key, value.ToString());
        }

        public void SetBoolValue(string key, bool value)
        {
            SetValue(key, value.ToString());
        }
    }
}