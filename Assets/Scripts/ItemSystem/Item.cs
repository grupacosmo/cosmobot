using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemInfo itemInfo;

        [SerializeField] public SerializableDictionary<string, string> ItemData;

        public ItemInfo ItemInfo => itemInfo;

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as integers.
        ///     Will throw if the value is not a valid integer or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid integer.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<int> IntValue => new(ItemData);

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as float.
        ///     Will throw if the value is not a valid float or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid float.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<float> FloatValue => new(ItemData);

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as bool.
        ///     Will throw if the value is not a valid bool or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid bool.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<bool> BoolValue => new(ItemData);

        public SerializableDictionary<string, string> StringValue => ItemData;

        private void Awake()
        {
            ComponentUtils.RequireNotNull(itemInfo, "itemInfo is not set.", this);
            InitItemData();
        }

        private void InitItemData()
        {
            ItemData ??= new SerializableDictionary<string, string>();

            foreach (var additionalField in itemInfo.AdditionalData)
                ItemData.TryAdd(additionalField.Key, additionalField.Value);
        }

        /// <summary>
        ///     Returns the value of the given key, or null if the key does not exist.
        /// </summary>
        public string GetValue(string key)
        {
            return ItemData.GetValueOrDefault(key, null);
        }

        public int? GetIntValue(string key)
        {
            var value = GetValue(key);
            if (value is not null && int.TryParse(value, out var result)) return result;
            return null;
        }

        public float? GetFloatValue(string key)
        {
            var value = GetValue(key);
            if (value is not null && float.TryParse(value, out var result)) return result;
            return null;
        }

        public bool? GetBoolValue(string key)
        {
            var value = GetValue(key);
            if (value is not null && bool.TryParse(value, out var result)) return result;
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
                    var b = (bool)Convert.ChangeType(value, typeof(bool));
                    return b ? "true" : "false"; // i hate C#
                }

                return value.ToString();
            }
        }
    }
}