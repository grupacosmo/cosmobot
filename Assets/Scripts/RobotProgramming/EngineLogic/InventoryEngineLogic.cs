using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cosmobot.Api.TypesInternal;
using Cosmobot.ItemSystem;
using Cosmobot.Utils;
using UnityEngine;

namespace Cosmobot.Api
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseEngineLogic))]
    [RequireComponent(typeof(InventoryComponent))]
    public class InventoryEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ProgrammableFunctionWrapper wrapper;

        private BaseEngineLogic baseLogic;

        private InventoryComponent inventoryComponent;
        [SerializeField] private float searchRange;
        [SerializeField] private float reachRange;

        private void Start()
        {
            inventoryComponent = gameObject.GetComponent<InventoryComponent>();
            baseLogic = gameObject.GetComponent<BaseEngineLogic>();
        }

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            wrapper = new ProgrammableFunctionWrapper(taskEvent, token, commandQueue);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            // Here you can expose the robot's functions in the game, use:
            // WrapOneFrame() for immediate functions and
            // WrapDeffered() for time-stretched functions (like coroutines)
            return new Dictionary<string, Delegate>()
            {
                { "findItem", wrapper.WrapOneFrame<string, Item>(FindItem)},
                { "findClosestItem", wrapper.WrapOneFrame<string, Item>(FindClosestItem)},
                { "findAllItems", wrapper.WrapOneFrame<string, List<Item>>(FindAllItems)},
                { "pickupItem", wrapper.WrapOneFrame<Item>(PickupItem)},
                { "dropItem", wrapper.WrapOneFrame<string>(DropItem)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to include "ManualResetEvent taskCompletedEvent" in arguments if using WrapDeffered and .Set() it at the end of action
        private Item FindItem(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 1 << Layers.ITEM);

            if (objects.Length == 0)
            {
                return null;
            }

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (itemComponent == null)
                    continue;

                if (string.IsNullOrEmpty(type) || itemComponent.ItemInfo.Id == type)
                {
                    Vector2 pos = new Vector2(collider.transform.position.x, collider.transform.position.z);
                    Item item = new Item(itemComponent, wrapper);
                    return item;
                }
            }

            return null;
        }

        private Item FindClosestItem(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 1 << Layers.ITEM);

            if (objects.Length == 0)
            {
                return null;
            }

            Item closest = null;
            float distance = searchRange;

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (itemComponent == null)
                    continue;

                if (string.IsNullOrEmpty(type) || itemComponent.ItemInfo.Id == type)
                {
                    float currentDist = Vector3.Distance(transform.position, collider.transform.position);
                    if (currentDist < distance)
                    {
                        distance = currentDist;
                        Vector2 pos = new Vector2(collider.transform.position.x, collider.transform.position.z);
                        closest = new Item(itemComponent, wrapper);
                    }
                }
            }

            return closest;
        }

        private List<Item> FindAllItems(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 1 << Layers.ITEM);
            List<Item> items = new List<Item>();

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (itemComponent == null)
                    continue;

                if (string.IsNullOrEmpty(type) || itemComponent.ItemInfo.Id == type)
                {
                    Vector2 pos = new Vector2(collider.transform.position.x, collider.transform.position.z);
                    items.Add(new Item(itemComponent, wrapper));
                }
            }

            return items;
        }

        private void PickupItem(Item item)
        {
            if (item == null || !item.itemComponent)
            {
                baseLogic.LogError("Item doesn't exist anymore");
                return;
            }

            Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
            if (Vector2.Distance(pos, item.itemComponent.transform.position) > reachRange)
            {
                baseLogic.LogError("Item is too far");
                return;
            }

            if (inventoryComponent.inventory.AddItem(item.itemComponent.Item))
            {
                item.itemComponent.Dispose();
                return;
            }

            baseLogic.LogError("Couldn't add item to inventory");
        }

        private void DropItem(string itemId = "")
        {
            if (string.IsNullOrEmpty(itemId))
            {
                ItemInstance temp = inventoryComponent.inventory.RemoveLatest();
                if (temp == null)
                {
                    baseLogic.Log("No items to be dropped");
                    return;
                }

                temp.ItemInfo.InstantiateItem(transform.position + transform.forward, Quaternion.identity);
                return;
            }

            ItemInstance item = inventoryComponent.inventory.RemoveFirstById(itemId);
            if (item == null)
            {
                baseLogic.Log("No such item in inventory");
                return;
            }

            item.ItemInfo.InstantiateItem(transform.position + transform.forward, Quaternion.identity);
            return;
        }
    }
}
