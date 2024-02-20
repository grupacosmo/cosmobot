using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    [DefaultExecutionOrder(ExecutionOrder.ItemManager)]
    public class ItemManager : SingletonSystem<ItemManager>
    {

        [SerializeField]
        private string[] itemsDataDirectory = new string[] { };

        private List<ItemInfo> items = new List<ItemInfo>();

        public IReadOnlyList<ItemInfo> Items => items.AsReadOnly();

        public ItemInfo GetItem(string id)
        {
            return items.FirstOrDefault(item => item.Id == id);
        }

        protected override void SystemAwake()
        {
            ValidateDirectories();
            LoadItems();
        }

        private void ValidateDirectories()
        {
            string invalidDirs =
                string.Join(
                    ",  ",
                    itemsDataDirectory.Where(dir => !AssetDatabase.IsValidFolder(dir)));

            if (invalidDirs.Length > 0)
                Debug.LogError($"Invalid directories: {invalidDirs}");
        }

        private void LoadItems()
        {
            // why Unity? I cant load asset by GUID directly and the only method that search in
            // directory returns GUIDs.
            List<ItemInfo> assets =
                AssetDatabase.FindAssets("t:ItemInfo", itemsDataDirectory)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => AssetDatabase.LoadAssetAtPath<ItemInfo>(path))
                    .ToList();


            List<ItemInfo> distinct =
                assets.Distinct(new FieldComparer<ItemInfo, string>(i => i.Id)).ToList();

            // idk if this is necessary
#if UNITY_EDITOR
            ValidateItems(assets, distinct);
#endif
            items = distinct;
        }

        private void ValidateItems(List<ItemInfo> assets, List<ItemInfo> distinct)
        {
            if (assets.Count == distinct.Count) return;

            string duplicates =
                string.Join(
                    ";\n",
                    assets
                        .GroupBy(i => i.Id)
                        .Where(g => g.Count() > 1)
                        .Select(DuplicateGroupToString));

            Debug.LogError($"Duplicate item IDs:\n{duplicates}");
        }

        private string DuplicateGroupToString(IGrouping<string, ItemInfo> group)
        {
            var values = group.Select(i => $"{i.DisplayName} ({i.name})");
            string joined = string.Join(", ", values);
            return $"{group.Key}: ${joined}";
        }

        private class FieldComparer<T, R> : IEqualityComparer<T>
        {
            private readonly Func<T, R> selector;
            public FieldComparer(Func<T, R> selector)
            {
                this.selector = selector;
            }
            public bool Equals(T x, T y)
            {
                return selector(x).Equals(selector(y));
            }
            public int GetHashCode(T obj)
            {
                return selector(obj).GetHashCode();
            }
        }
    }
}