using System;
using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    [Serializable]
    public class CraftingRecipeSerializationGroup
    {
        /// <summary> Internal ID </summary>
        public string id;

        /// <summary> Display name </summary>
        public string name;

        public List<string> recipes;
    }
}
