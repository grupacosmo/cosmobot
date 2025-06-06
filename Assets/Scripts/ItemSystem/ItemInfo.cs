using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    [CreateAssetMenu(fileName = "ItemInfo", menuName = "Cosmobot/ItemSystem/ItemInfo", order = 1)]
    public class ItemInfo : ScriptableObject, IEquatable<ItemInfo>
    {

        [SerializeField]
        private string id;
        [SerializeField]
        private string displayName;
        [SerializeField]
        private Texture2D icon;
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private SerializableDictionary<string, string> additionalData = new();

        public string Id => id;
        public string DisplayName => displayName;
        public Texture2D Icon => icon;
        public GameObject Prefab => prefab;
        public IReadOnlyDictionary<string, string> AdditionalData => additionalData;

        public GameObject InstantiateItem(Vector3 position, Quaternion rotation)
        {
            GameObject instantiatedObject = Instantiate(Prefab, position, rotation);
            ItemComponent itemComponent = instantiatedObject.GetComponent<ItemComponent>();
            if (!itemComponent)
            {
                throw new InvalidOperationException(
                    $"Prefab of item '{id}' does not have '{nameof(ItemComponent)}' script attached!");
            }
            if (itemComponent.ItemInfo != this)
            {
                throw new InvalidOperationException(
                    $"Prefab of item '{id}' has different ItemInfo setup! (found '{itemComponent.ItemInfo.Id}')");
            }
            foreach (KeyValuePair<string,string> keyValuePair in additionalData)
            {
                itemComponent.ItemData[keyValuePair.Key] = keyValuePair.Value;
            }
            return instantiatedObject;
        }

        public override bool Equals(object obj) => Equals(obj as ItemInfo);

        public bool Equals(ItemInfo other) => other != null && id == other.id;

        public override int GetHashCode() => id.GetHashCode();

        public static bool operator ==(ItemInfo left, ItemInfo right)
        {
            if (ReferenceEquals(left, right)) return true;
            return left is not null && right is not null && left.Equals(right);
        }

        public static bool operator !=(ItemInfo left, ItemInfo right) => !(left == right);
    }
}
