using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    [Serializable]
    public class ItemInstance
    {
        [SerializeField]
        private ItemInfo itemInfo;

        [SerializeField]
        private SerializableDictionary<string, string> itemData;

        public ItemInstance(ItemInfo itemInfo)
        {
            this.itemInfo = itemInfo;
            ItemData = new SerializableDictionary<string, string>();

            foreach (var additionalField in itemInfo.AdditionalData)
                ItemData.TryAdd(additionalField.Key, additionalField.Value);
        }

        /// <summary>
        ///     This constructor is for serialization purposes only.
        /// </summary>
        [Obsolete("This constructor is for serialization purposes only.")]
        public ItemInstance()
        {
            ItemData = new SerializableDictionary<string, string>();
        }

        public ItemInfo ItemInfo => itemInfo;

        public SerializableDictionary<string, string> ItemData
        {
            get => itemData;
            set => itemData = value ?? new SerializableDictionary<string, string>();
        }

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as integers.
        ///     Will throw if the value is not a valid integer or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid integer.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<int> IntValue => new(itemData);

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as float.
        ///     Will throw if the value is not a valid float or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid float.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<float> FloatValue => new(itemData);

        /// <summary>
        ///     <c>ItemData</c> accessor that treats the values as bool.
        ///     Will throw if the value is not a valid bool or if the key does not exist when
        ///     trying to get the value.
        /// </summary>
        /// <exception cref="FormatException">Thrown when the value is not a valid bool.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the key does not exist.</exception>
        public ValueAccessor<bool> BoolValue => new(itemData);

        public SerializableDictionary<string, string> StringValue => itemData;

        public string Id => itemInfo.Id;


        /// <summary>
        ///     Returns the value of the given key, or null if the key does not exist.
        /// </summary>
        public string GetValue(string key)
        {
            return ItemData.GetValueOrDefault(key, null);
        }

        public int? GetIntValue(string key)
        {
            return SerializationUtils.TryParse(GetValue(key), out int result) ? result : null;
        }

        public float? GetFloatValue(string key)
        {
            return SerializationUtils.TryParse(GetValue(key), out float result) ? result : null;
        }

        public bool? GetBoolValue(string key)
        {
            return SerializationUtils.TryParse(GetValue(key), out bool result) ? result : null;
        }

        public void SetValue(string key, string value)
        {
            ItemData[key] = value;
        }

        public void SetIntValue(string key, int? value)
        {
            SetValue(key, SerializationUtils.ToString(value));
        }

        public void SetFloatValue(string key, float? value)
        {
            SetValue(key, SerializationUtils.ToString(value));
        }

        public void SetBoolValue(string key, bool? value)
        {
            SetValue(key, SerializationUtils.ToString(value));
        }
    }
}
