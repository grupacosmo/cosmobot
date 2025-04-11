using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot.Research
{
    public class ResearchUnlockedRecipes
    {
        private static ResearchUnlockedRecipes _instance;
        public static ResearchUnlockedRecipes Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ResearchUnlockedRecipes();
                }
                return _instance;
            }
        }
        private HashSet<string> unlockedRecipes = new HashSet<string>();

        private ResearchUnlockedRecipes()
        {
        }

        public void Add(string recipeId)
        {
            unlockedRecipes.Add(recipeId);
        }

        public void Remove(string recipeId)
        {
            unlockedRecipes.Remove(recipeId);
        }

        public bool IsRecipeUnlocked(string recipeId)
        {
            return unlockedRecipes.Contains(recipeId);
        }

        public IEnumerable<string> GetAllUnlockedRecipes()
        {
            return unlockedRecipes;
        }

        public void ResetAll()
        {
            unlockedRecipes.Clear();
        }
    }
}
