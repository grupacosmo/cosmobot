using UnityEngine;

namespace Cosmobot.ItemSystem
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryDisplay : MonoBehaviour
    {
        private Inventory inventory;

        private void Awake()
        {
            inventory = GetComponent<Inventory>();
        }

        private void OnEnable()
        {
            inventory.OnItemAdded += OnInventoryChange;
            inventory.OnItemRemoved += OnInventoryChange;
            inventory.OnItemProcessed += OnInventoryChange;
        }
        
        private void OnDisable()
        {
            inventory.OnItemAdded -= OnInventoryChange;
            inventory.OnItemRemoved -= OnInventoryChange;
            inventory.OnItemProcessed -= OnInventoryChange;
        }
        
        private void OnInventoryChange(Inventory source, ItemInstance item)
        {
            RedrawInventory();
        }

        private void RedrawInventory()
        {
            // todo draw inventory
        }
    }
}