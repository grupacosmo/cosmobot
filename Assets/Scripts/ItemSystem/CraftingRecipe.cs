using System;
using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    [Serializable]
    public struct CraftingRecipe
    {
        /// <summary> Internal ID </summary>
        public string Id;

        /// <summary> Display name </summary>
        public string Name;

        public List<string> Ingredients;
        public List<string> Result;
        public int EnergyCost;
    }
}
