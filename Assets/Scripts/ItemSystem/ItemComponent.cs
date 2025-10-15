using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    public class ItemComponent : MonoBehaviour
    {
        [SerializeField]
        private ItemInstance item;
        public ItemInstance Item
        {
            get => item;
            private set => item = value;
        }

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

#if UNITY_ENGINE
        private void Awake()
        {
            ComponentUtils.RequireNotNull(item, "'item' is not set.", this);
        }
#endif

        void Init(ItemInstance initValue)
        {
            if (item is not null)
            {
                throw new InvalidOperationException("Can't re initialise ItemComponent that is already initialised");
            }
            item = initValue;
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

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}
