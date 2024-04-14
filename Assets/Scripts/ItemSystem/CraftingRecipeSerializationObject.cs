using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    [System.Serializable]
    public class CraftingRecipeSerializationObject
    {
        public List<CraftingRecipe> Recipes;
        public List<CraftingRecipeSerializationGroup> Groups;
    }
}