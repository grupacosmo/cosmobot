using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cosmobot.ItemSystem
{
    public class Crafter : MonoBehaviour
    {
        [SerializeField] private string craftingRecipeGroupId;

        [Tooltip("Can be null if the crafter doesn't require energy to craft items.")] [SerializeReference]
        private MonoBehaviourEnergyInterface energyInterface;

        [SerializeField] private List<Transform> outputSlots;

#if UNITY_EDITOR
        // TODO: temporary debug tool
        [SerializeField] private bool clickToCraft;
#endif


        private CraftingRecipeGroup craftingRecipeGroup;

        private readonly List<Collider> itemsCollidersInInputSlot = new();

        private ISet<Item> ItemsInInputSlot =>
            itemsCollidersInInputSlot
                .Select(c => c.GetComponent<Item>())
                .Where(i => i is not null)
                .ToHashSet();

        private void Awake()
        {
            craftingRecipeGroup = ItemManager.Instance.GetCraftingRecipeGroup(craftingRecipeGroupId);
        }

#if UNITY_EDITOR
        // TODO: temporary debug tool
        private void Update()
        {
            if (clickToCraft)
            {
                clickToCraft = false;
                Craft();
            }
        }
#endif

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var slot in outputSlots) Gizmos.DrawSphere(slot.position, 0.1f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Tags.Item)) itemsCollidersInInputSlot.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(Tags.Item)) itemsCollidersInInputSlot.Remove(other);
        }

        /// <summary>
        ///     Tries to craft an item using the items in the input slot.
        ///     If no recipe is found, it will do nothing.
        ///     If a recipe is found, it will instantiate the result items in the output slots.
        /// </summary>
        public void Craft()
        {
            CraftingRecipe? recipeOp = GetCraftingRecipeForItemsInSlot();
            if (recipeOp is null)
            {
                Debug.Log("No recipe found for the items in the input slot or the output slots are not " +
                          "enough to hold the result items");
                return;
            }

            CraftingRecipe recipe = recipeOp.Value;

            if (recipe.EnergyCost > 0 &&
                !((IEnergyInterface)energyInterface).TransferEnergyOutOrFail(recipe.EnergyCost))
            {
                Debug.Log("Not enough energy to craft the item");
                return;
            }

            int outputSlotIndex = 0;
            foreach (string resultItemId in recipe.Result)
            {
                ItemInfo resultItem = ItemManager.Instance.GetItem(resultItemId);
                resultItem.InstantiateItem(outputSlots[outputSlotIndex].position, Quaternion.identity);
                outputSlotIndex++;
            }

            foreach (GameObject itemGo in itemsCollidersInInputSlot.Select(collider => collider.gameObject))
                Destroy(itemGo);
            // coz OnTriggerExit is not called when destroying colliders
            itemsCollidersInInputSlot.Clear();
        }

        // will return the first recipe that can be crafted with the items in the input slot and with result items that
        // can fit in the output slots. If no recipe is found, it will return null.
        private CraftingRecipe? GetCraftingRecipeForItemsInSlot()
        {
            if (itemsCollidersInInputSlot.Count == 0) return null;

            bool energyInterfaceAvailable = energyInterface is not null;
            foreach (var recipe in craftingRecipeGroup.Recipes)
            {
                if (recipe.Result.Count > outputSlots.Count) continue;
                if (recipe.EnergyCost > 0 && !energyInterfaceAvailable) continue;

                var itemsIdsInInputSlot = ItemsInInputSlot.Select(iis => iis.ItemInfo.Id);
                if (recipe.Ingredients.OrderBy(x => x).SequenceEqual(itemsIdsInInputSlot.OrderBy(x => x)))
                    return recipe;
            }

            return null;
        }
    }
}