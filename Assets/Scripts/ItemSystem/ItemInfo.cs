using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    [CreateAssetMenu(fileName = "ItemInfo", menuName = "Cosmobot/ItemSystem/ItemInfo", order = 1)]
    public class ItemInfo : ScriptableObject
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
            Item item = instantiatedObject.GetComponent<Item>();
            if (!item)
            {
                throw new InvalidOperationException(
                    $"Prefab of item '{id}' does not have '{nameof(Item)}' script attached!");
            }
            item.ItemData = new SerializableDictionary<string, string>(additionalData);
            return instantiatedObject;
        }

    }
}
