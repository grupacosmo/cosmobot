using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cosmobot.Utils;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    [DefaultExecutionOrder(ExecutionOrder.ItemManager)]
    public class ItemManager : SingletonSystem<ItemManager>
    {
        //[SerializeField]
        //private string craftingRecipesDataDirectory = "/Data/Crafting";
        [SerializeField]
        private TextAsset recipesJson;

        private List<CraftingRecipe> craftingRecipes;
        private List<CraftingRecipeGroup> craftingRecipesGroups;

        [SerializeField]
        private List<ItemInfo> items;

        public IReadOnlyList<ItemInfo> Items => items.AsReadOnly();
        public IReadOnlyList<CraftingRecipe> CraftingRecipes => craftingRecipes.AsReadOnly();
        public IReadOnlyList<CraftingRecipeGroup> CraftingRecipesGroups => craftingRecipesGroups.AsReadOnly();

        public ItemInfo GetItem(string id)
        {
            return items.FirstOrDefault(item => item.Id == id);
        }

        public CraftingRecipe? GetCraftingRecipe(string id)
        {
            foreach (var recipe in craftingRecipes)
                if (recipe.id == id)
                    return recipe;

            return null;
        }

        [CanBeNull]
        public CraftingRecipeGroup GetCraftingRecipeGroup(string id)
        {
            return craftingRecipesGroups.FirstOrDefault(group => group.Id == id);
        }

        protected override void SystemAwake()
        {
            //craftingRecipesDataDirectory = Path.Combine(Application.dataPath, craftingRecipesDataDirectory);

            ValidateRecipeDirectory();
            LoadItems();
            LoadCraftingRecipes();
        }

        private void ValidateRecipeDirectory()
        {
            //if (!File.Exists(craftingRecipesDataDirectory))
            //{
            //    Debug.LogError($"Invalid directory for recipes: {craftingRecipesDataDirectory}");
            //}
        }

        private void LoadItems()
        {
            List<ItemInfo> distinct =
                items.Distinct(new FieldComparer<ItemInfo, string>(i => i.Id)).ToList();

            // idk if this is necessary
#if UNITY_EDITOR
            ValidateItems(items, distinct);
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

        private void LoadCraftingRecipes()
        {
            //CraftingRecipeSerializationObject deserializedObject =
            //     JsonUtility.FromJson<CraftingRecipeSerializationObject>(craftingRecipesDataDirectory);

            CraftingRecipeSerializationObject deserializedObject = null;

            if (recipesJson != null)
            {
                deserializedObject = JsonUtility.FromJson<CraftingRecipeSerializationObject>(recipesJson.text);
            }

            if (deserializedObject is null)
            {
                Debug.LogError("Failed to load crafting recipes.");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                // just for now
                // TODO: replace with proper error handling
                Application.Quit();
#endif
                return;
            }

            craftingRecipes = deserializedObject.recipes;
            Dictionary<string, CraftingRecipe> craftingRecipesMap = craftingRecipes.ToDictionary(r => r.id, r => r);
            craftingRecipesGroups = deserializedObject.groups.Select(g =>
            {
                List<CraftingRecipe> recipes = g.recipes.Select(rId => craftingRecipesMap[rId]).ToList();
                return new CraftingRecipeGroup(g.id, g.name, recipes);
            }).ToList();
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
