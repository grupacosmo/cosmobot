using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    public class CraftingRecipeGroup
    {
        public readonly string Id;
        public readonly string Name;
        public readonly IReadOnlyList<CraftingRecipe> Recipes;

        public CraftingRecipeGroup(string id, string name, List<CraftingRecipe> recipes)
        {
            Id = id;
            Name = name;
            Recipes = recipes.AsReadOnly();
        }
    }
}
