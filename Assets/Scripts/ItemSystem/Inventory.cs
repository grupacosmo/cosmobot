using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    public class Inventory : MonoBehaviour
    {
        public delegate void InventoryChanged(Inventory source, ItemInstance eventItem);

        [SerializeField]
        private int capacity;

        private readonly List<ItemInstance> items = new();

        public event InventoryChanged OnItemAdded;
        public event InventoryChanged OnItemRemoved;
        public event InventoryChanged OnItemProcessed;

        public int ItemCount => items.Count;

        // There is no GetItemInstance because we don't want callers to modify the item instance directly.
        // Instead, we provide methods to interact with the inventory.

        public ItemInfo GetItemInfo(int index)
        {
            return items[index].ItemInfo;
        }

        public IReadOnlyDictionary<string, string> GetItemData(int index)
        {
            return items[index].ItemData;
        }

        public bool AddItem(ItemInstance item)
        {
            if (items.Count >= capacity) return false;
            items.Add(item);
            OnItemAdded?.Invoke(this, item);
            return true;
        }

        public bool RemoveItem(ItemInstance item)
        {
            bool removed = items.Remove(item);
            if (removed) OnItemRemoved?.Invoke(this, item);
            return removed;
        }

        [CanBeNull]
        private ItemInstance RemoveFirstById(string id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                {
                    var item = items[i];
                    items.RemoveAt(i);
                    OnItemRemoved?.Invoke(this, item);
                    return item;
                }
            }

            return null;
        }

        public bool HasItemById(string itemId)
        {
            return items.Any(item => item.Id == itemId);
        }

        public int CountItemsById(string itemId)
        {
            return items.Count(item => item.Id == itemId);
        }

        public bool ProcessFirstItemWithId(string itemId, System.Action<ItemInstance> process)
        {
            var item = items.FirstOrDefault(i => i.Id == itemId);
            if (item is not null)
            {
                process(item);
                OnItemProcessed?.Invoke(this, item);
                return true;
            }

            return false;
        }
    }
}