using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    [Serializable]
    public class Inventory
    {
        public delegate void InventoryChanged(Inventory source, ItemInstance eventItem);

        private static readonly Predicate<ItemInstance> allowAllFilter = _ => true;

        [SerializeField]
        public bool allowAddingItems = true;

        [SerializeField]
        public bool allowRemovingItems = true;

        [SerializeField]
        private int capacity;

        private readonly List<ItemInstance> items = new();

        private Predicate<ItemInstance> itemFilter = allowAllFilter;

        // There is no GetItemInstance because we don't want callers to modify the item instance directly.
        // Instead, Class provides methods to interact with the inventory.

        public Inventory(int capacity)
        {
            this.capacity = capacity;
            items.Capacity = capacity;
        }

        public int Capacity => capacity;
        public int ItemCount => items.Count;

        /// <summary>
        ///     Limits items that can be added to inventory. `null` is translated to allow all (`(_) => true`)
        /// </summary>
        public Predicate<ItemInstance> ItemFilter
        {
            get => itemFilter;
            set => itemFilter = value ?? allowAllFilter;
        }

        public event InventoryChanged OnItemAdded;
        public event InventoryChanged OnItemRemoved;
        public event InventoryChanged OnItemProcessed;

        public bool TryGetItem(int index, out ItemInfo item)
        {
            if (index < 0 || index >= items.Count)
            {
                item = null;
                return false;
            }

            item = GetItemInfo(index);
            return true;
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or <paramref name="index" /> is equal to or greater than
        ///     <see cref="ItemCount" />
        /// </exception>
        public ItemInfo GetItemInfo(int index)
        {
            return items[index].ItemInfo;
        }

        public bool TryGetItemData(int index, out IReadOnlyDictionary<string, string> itemData)
        {
            if (index < 0 || index >= items.Count)
            {
                itemData = null;
                return false;
            }

            itemData = GetItemData(index);
            return true;
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than 0 or <paramref name="index" /> is equal to or greater than
        ///     <see cref="ItemCount" />
        /// </exception>
        public IReadOnlyDictionary<string, string> GetItemData(int index)
        {
            return items[index].ItemData;
        }

        public bool AddItem(ItemInstance item)
        {
            if (!allowAddingItems) return false;
            if (items.Count >= capacity) return false;
            if (!itemFilter.Invoke(item)) return false;

            items.Add(item);
            OnItemAdded?.Invoke(this, item);
            return true;
        }

        public bool RemoveItem(ItemInstance item)
        {
            if (!allowRemovingItems) return false;

            bool removed = items.Remove(item);
            if (removed) OnItemRemoved?.Invoke(this, item);
            return removed;
        }

        [CanBeNull]
        public ItemInstance RemoveFirstById(string id)
        {
            if (!allowRemovingItems) return null;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                {
                    ItemInstance item = items[i];
                    items.RemoveAt(i);
                    OnItemRemoved?.Invoke(this, item);
                    return item;
                }
            }

            return null;
        }

        [CanBeNull]
        public ItemInstance RemoveFirstByFilter(Predicate<ItemInstance> filter)
        {
            if (!allowRemovingItems) return null;

            for (int i = 0; i < items.Count; i++)
            {
                if (filter.Invoke(items[i]))
                {
                    ItemInstance item = items[i];
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

        public bool HasItemByFilter(Predicate<ItemInstance> filter)
        {
            return items.Any(filter.Invoke);
        }

        public int CountItemsById(string itemId)
        {
            return items.Count(item => item.Id == itemId);
        }

        /// <returns> true if an item was processed, false if no item was found </returns>
        public bool ProcessFirstItemWithId(string itemId, Action<ItemInstance> process)
        {
            ItemInstance item = items.FirstOrDefault(i => i.Id == itemId);
            if (item is not null)
            {
                process(item);
                OnItemProcessed?.Invoke(this, item);
                return true;
            }

            return false;
        }


        /// <summary>
        ///     Processes all items with the given id. Function first caches all items with the given id, then processes
        ///     them with <i>process</i> Action and finally invokes OnItemProcessed event for each item.
        ///     It is safe to change inventory content during processing and adding/removing items will not affect the
        ///     processing. But any OnItemAdded/OnItemRemoved events will be invoked immediately.
        ///     For example, if you process items with id "A" and during processing you remove another item with same
        ///     id "A", the removed item will still be processed and OnItemRemoved event will be invoked. At the end the
        ///     OnItemProcessed event will be invoked for all processed items (including the removed one).
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="process"></param>
        /// <returns> number of items processed. 0 if no items were found </returns>
        public int ProcessAllItemsWithId(string itemId, Action<ItemInstance> process)
        {
            List<ItemInstance> itemInstances = items.Where(i => i.Id == itemId).ToList();
            foreach (ItemInstance item in itemInstances)
            {
                process(item);
            }

            foreach (ItemInstance item in itemInstances)
            {
                OnItemProcessed?.Invoke(this, item);
            }

            return itemInstances.Count;
        }
    }
}
