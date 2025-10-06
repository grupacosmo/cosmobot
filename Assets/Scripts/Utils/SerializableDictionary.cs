using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cosmobot
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>
    {
        [SerializeField]
        private List<TKey> keys = new();

        [SerializeField]
        private List<TValue> values = new();

        public SerializableDictionary() { }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public ICollection<TKey> Keys => keys.AsReadOnly();

        public ICollection<TValue> Values => values.AsReadOnly();

        public int Count => keys.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get => GetValue(key);
            set => Add(key, value);
        }

        public void Add(TKey key, TValue value)
        {
            int keyIndex = keys.IndexOf(key);
            if (keyIndex != -1)
            {
                values[keyIndex] = value;
            }
            else
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return keys.Contains(key);
        }

        public bool Remove(TKey key)
        {
            int keyIndex = keys.IndexOf(key);
            if (keyIndex != -1)
            {
                keys.RemoveAt(keyIndex);
                values.RemoveAt(keyIndex);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int keyIndex = keys.IndexOf(key);
            if (keyIndex != -1)
            {
                value = values[keyIndex];
                return true;
            }

            value = default;
            return false;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return keys.Contains(item.Key) && values.Contains(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array is null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < Count) throw new ArgumentException("Array is too small!");

            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ContainsValue(item.Value) && Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return keys.Zip(values, (key, value) => new KeyValuePair<TKey, TValue>(key, value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => keys.AsReadOnly();

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => values.AsReadOnly();


        public TValue GetValue(TKey key)
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Key '{key}' not found in dictionary!");
        }

        public bool ContainsValue(TValue value)
        {
            return values.Contains(value);
        }
    }
}
