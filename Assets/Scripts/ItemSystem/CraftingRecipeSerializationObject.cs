using System;
using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    [Serializable]
    public class CraftingRecipeSerializationObject
    {
        public List<CraftingRecipe> Recipes;
        public List<CraftingRecipeSerializationGroup> Groups;
    }
}
