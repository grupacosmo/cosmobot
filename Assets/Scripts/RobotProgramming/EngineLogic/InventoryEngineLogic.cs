using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cosmobot.Api.Types;
using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot.Api
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseEngineLogic))]
    [RequireComponent(typeof(InventoryComponent))]
    public class InventoryEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private ProgrammableFunctionWrapper wrapper;

        private InventoryComponent inventoryComponent;
        private BaseEngineLogic baseLogic;
        [SerializeField] private float searchRange;
        [SerializeField] private float reachRange;

        private void Start()
        {
            inventoryComponent = gameObject.GetComponent<InventoryComponent>();
            baseLogic = gameObject.GetComponent<BaseEngineLogic>();
        }

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            taskCompletedEvent = taskEvent;
            cancellationToken = token;
            wrapper = new ProgrammableFunctionWrapper(taskCompletedEvent, cancellationToken, commandQueue);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            //Expose robot's ingame functions here
            return new Dictionary<string, Delegate>()
            {
                //{ "ExampleFunctionARG", wrapper.Wrap(ExampleFunctionARG)},
                //{ ... },
                { "FindItem", wrapper.Wrap<string, Item>(FindItem)},
                { "FindClosestItem", wrapper.Wrap<string, Item>(FindClosestItem)},
                { "FindAllItems", wrapper.Wrap<string, List<Item>>(FindAllItems)},
                { "PickupItem", wrapper.Wrap<Item>(PickupItem)},
                { "DropItem", wrapper.Wrap<string>(DropItem)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        Item FindItem(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 1 << GlobalConstants.ITEM_LAYER);

            if (objects.Length == 0)
            {
                taskCompletedEvent.Set();
                return null;
            }

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (itemComponent == null)
                    continue;

                if (string.IsNullOrEmpty(type) || itemComponent.ItemInfo.Id == type)
                {
                    taskCompletedEvent.Set();
                    Vector2 pos = new Vector2(collider.transform.position.x, collider.transform.position.z);
                    return new Item(itemComponent, pos);
                }
            }

            taskCompletedEvent.Set();
            return null;
        }

        Item FindClosestItem(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 1 << GlobalConstants.ITEM_LAYER);

            if (objects.Length == 0)
            {
                taskCompletedEvent.Set();
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
                        closest = new Item(itemComponent, pos);
                    }
                }
            }

            taskCompletedEvent.Set();
            return closest;
        }

        List<Item> FindAllItems(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 1 << GlobalConstants.ITEM_LAYER);
            List<Item> items = new List<Item>();

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (itemComponent == null)
                    continue;

                if (string.IsNullOrEmpty(type) || itemComponent.ItemInfo.Id == type)
                {
                    Vector2 pos = new Vector2(collider.transform.position.x, collider.transform.position.z);
                    items.Add(new Item(itemComponent, pos));
                }
            }

            taskCompletedEvent.Set();
            return items;
        }

        void PickupItem(Item item)
        {
            if (item == null || item.itemComponent == null || !item.IsValid)
            {
                baseLogic.LogError("Item doesn't exist anymore");
                taskCompletedEvent.Set();
                return;
            }

            Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
            if (Vector2.Distance(pos, item.Position) > reachRange)
            {
                baseLogic.LogError("Item is too far");
                taskCompletedEvent.Set();
                return;
            }

            if (inventoryComponent.inventory.AddItem(item.itemComponent.Item))
            {
                item.itemComponent.Dispose();
                taskCompletedEvent.Set();
                return;
            }

            baseLogic.LogError("Couldn't add item to inventory");
            taskCompletedEvent.Set();
        }

        void DropItem(string itemId = "")
        {
            if (string.IsNullOrEmpty(itemId))
            {
                ItemInstance temp = inventoryComponent.inventory.RemoveLatest();
                if (temp == null)
                {
                    baseLogic.Log("No items to be dropped");
                    taskCompletedEvent.Set();
                    return;
                }

                Instantiate(temp.ItemInfo.Prefab, transform.position + transform.forward, Quaternion.identity);
                taskCompletedEvent.Set();
                return;
            }

            ItemInstance item = inventoryComponent.inventory.RemoveFirstById(itemId);
            if (item == null)
            {
                baseLogic.Log("No such item in inventory");
                taskCompletedEvent.Set();
                return;
            }

            Instantiate(item.ItemInfo.Prefab, transform.position + transform.forward, Quaternion.identity);
            taskCompletedEvent.Set();
            return;
        }
    }
}
