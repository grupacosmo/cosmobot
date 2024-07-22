using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    public class ItemComponent : MonoBehaviour
    {
        [SerializeField] 
        private ItemInstance item;

        public ItemInfo ItemInfo => item.ItemInfo;

        public SerializableDictionary<string, string> ItemData => item.StringValue;
        
        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as integers.
        ///     Will throw if the value is not a valid integer or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid integer.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<int> IntValue => item.IntValue;

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as float.
        ///     Will throw if the value is not a valid float or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid float.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<float> FloatValue => item.FloatValue;

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as bool.
        ///     Will throw if the value is not a valid bool or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid bool.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<bool> BoolValue => item.BoolValue;

        public SerializableDictionary<string, string> StringValue => item.StringValue;

        private void Awake()
        {
            ComponentUtils.RequireNotNull(item, "'item' is not set.", this);
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
    }
}