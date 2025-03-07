using System.Collections.Generic;

namespace Cosmobot.ItemSystem
{
    [System.Serializable]
    public class CraftingRecipeSerializationGroup
    {
        /// <summary> Internal ID </summary>
        public string Id;
        /// <summary> Display name </summary>
        public string Name;
        public List<string> Recipes;
    }
}