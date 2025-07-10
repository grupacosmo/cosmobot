using System;
using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    [Serializable]
    public class CraftingRecipeSerializationGroup
    {
        /// <summary> Internal ID </summary>
        public string Id;

        /// <summary> Display name </summary>
        public string Name;

        public List<string> Recipes;
    }
}
