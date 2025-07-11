using System;
using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    [Serializable]
    public struct CraftingRecipe
    {
        /// <summary> Internal ID </summary>
        public string id;

        /// <summary> Display name </summary>
        public string name;

        public List<string> ingredients;
        public List<string> result;
        public int energyCost;
    }
}
