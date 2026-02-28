using Cosmobot.Utils;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] private ItemInfo itemInfo;
        [SerializeField] private SerializableDictionary<string, string> itemData = new SerializableDictionary<string, string>();
        
        public ItemInfo ItemInfo => itemInfo;
        public SerializableDictionary<string, string> ItemData => itemData;
        
        private void Awake()
        {
            Debug.Log("ItemSpawner awake");
            if (Application.isPlaying)
            {
                ItemComponent itemComponent = gameObject.AddComponent<ItemComponent>();
                itemComponent.Init(new ItemInstance(itemInfo, itemData));
                Destroy(this);
            }
        }
    }
}
