using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cosmobot.Api.Types;
using Cosmobot.ItemSystem;

namespace Cosmobot
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

        void Start()
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
                { "FindItem", wrapper.Wrap<string, Item?>(FindItem)},
                { "FindClosestItem", wrapper.Wrap<string, Item?>(FindClosestItem)},
                { "FindAllItems", wrapper.Wrap<string, List<Item>>(FindAllItems)},
                { "PickupItem", wrapper.Wrap<Item>(PickupItem)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        Item? FindItem(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 10);

            if (objects.Length == 0)
            {
                taskCompletedEvent.Set();
                return null;
            }

            if (string.IsNullOrEmpty(type))
            {
                ItemComponent itemComponent = objects[0].GetComponent<ItemComponent>();
                if (!itemComponent)
                {
                    taskCompletedEvent.Set();
                    return null;
                }

                taskCompletedEvent.Set();
                Vector2 pos = new Vector2(objects[0].transform.position.x, objects[0].transform.position.z);
                return new Item(itemComponent, pos);
            }

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (!itemComponent)
                    continue;

                if (itemComponent.ItemInfo.Id == type)
                {
                    taskCompletedEvent.Set();
                    Vector2 pos = new Vector2(objects[0].transform.position.x, objects[0].transform.position.z);
                    return new Item(itemComponent, pos);
                }
            }

            taskCompletedEvent.Set();
            return null;
        }

        Item? FindClosestItem(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 10);

            if (objects.Length == 0)
            {
                taskCompletedEvent.Set();
                return null;
            }

            Item? closest = null;
            float distance = searchRange;

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (!itemComponent)
                    continue;

                if (itemComponent.ItemInfo.Id == type || string.IsNullOrEmpty(type))
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
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, searchRange, 10);
            List<Item> items = new List<Item>();

            foreach (Collider collider in objects)
            {
                ItemComponent itemComponent = collider.GetComponent<ItemComponent>();

                if (!itemComponent)
                    continue;

                if (itemComponent.ItemInfo.Id == type || string.IsNullOrEmpty(type))
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
            Vector2 pos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
            if(Vector2.Distance(pos, item.position.Value) <= reachRange)
            {
                if(inventoryComponent.inventory.AddItem(item.itemComponent.item))
                {
                    taskCompletedEvent.Set();
                    return;
                }

                baseLogic.LogError("Couldn't add item to invetory");
            }

            baseLogic.LogError("Item is too far");
        }
    }
}