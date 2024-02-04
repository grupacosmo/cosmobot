using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
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
        /// <c>ItemData</c> accessor that treats the values as integers. 
        /// Will throw if the value is not a valid integer or if the key does not exist when 
        /// trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid integer.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<int> IntValue => new ValueAccessor<int>(ItemData);

        /// <summary>
        /// <c>ItemData</c> accessor that treats the values as float. 
        /// Will throw if the value is not a valid float or if the key does not exist when 
        /// trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid float.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<float> FloatValue => new ValueAccessor<float>(ItemData);

        /// <summary>
        /// <c>ItemData</c> accessor that treats the values as bool. 
        /// Will throw if the value is not a valid bool or if the key does not exist when 
        /// trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid bool.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<bool> BoolValue => new ValueAccessor<bool>(ItemData);

        public SerializableDictionary<string, string> StringValue => ItemData;

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

        public void SetIntValue(string key, int? value)
        {
            SetValue(key, value.ToString());
        }

        public void SetFloatValue(string key, float? value)
        {
            SetValue(key, value.ToString());
        }

        public void SetBoolValue(string key, bool? value)
        {
            SetValue(key, value.ToString());
        }


        // helper
        public class ValueAccessor<T>
        {
            private readonly SerializableDictionary<string, string> array;

            public ValueAccessor(SerializableDictionary<string, string> array)
            {
                this.array = array;
            }

            public T this[string index]
            {
                get => (T)Convert.ChangeType(array[index], typeof(T));
                set => array[index] = ValueToString(value);
            }

            private string ValueToString(T value)
            {
                if (value is null) return null;
                if (typeof(T) == typeof(bool))
                {
                    bool b = (bool)Convert.ChangeType(value, typeof(bool));
                    return b ? "true" : "false"; // i hate C#
                }
                return value.ToString();
            }
        }
    }
}