using System;
using System.Collections.Generic;
using Cosmobot.ItemSystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Cosmobot
{
    public class InventoryTest
    {
        private ItemInfo testItemInfoBar;
        private ItemInfo testItemInfoFoo;

        [SetUp]
        public void SetUp()
        {
            testItemInfoFoo = CreateItemInfo("foo");
            testItemInfoBar = CreateItemInfo("bar");
        }

        private ItemInfo CreateItemInfo(string id)
        {
            ItemInfo itemInfo = ScriptableObject.CreateInstance<ItemInfo>();
            SerializedObject serializedObject = new SerializedObject(itemInfo);
            serializedObject.FindProperty("id").stringValue = id;
            serializedObject.FindProperty("displayName").stringValue = id;
            serializedObject.ApplyModifiedProperties();
            return itemInfo;
        }

        [Test]
        public void CreateInventoryAndItsEmpty()
        {
            Inventory inventory = new Inventory(10);
            Assert.AreEqual(10, inventory.Capacity, "capacity");
            Assert.Zero(inventory.ItemCount, "item count");
        }

        [Test]
        public void AddItemToInventoryAndAccessThem()
        {
            Inventory inventory = new Inventory(10);

            Assert.True(inventory.AddItem(new ItemInstance(testItemInfoFoo)));
            Assert.AreEqual(1, inventory.ItemCount);
            Assert.AreEqual(testItemInfoFoo, inventory.GetItemInfo(0));

            Assert.True(inventory.AddItem(new ItemInstance(testItemInfoBar)));
            Assert.AreEqual(2, inventory.ItemCount);
            Assert.AreEqual(testItemInfoBar, inventory.GetItemInfo(1));
        }

        [Test]
        public void AddItemToInventoryOverCapacityAndGetFalse()
        {
            Inventory inventory = new Inventory(2);

            inventory.AddItem(new ItemInstance(testItemInfoFoo));
            inventory.AddItem(new ItemInstance(testItemInfoBar));
            Assert.AreEqual(2, inventory.ItemCount);

            // over capacity
            Assert.False(inventory.AddItem(new ItemInstance(testItemInfoFoo)));
            Assert.AreEqual(2, inventory.ItemCount);
        }

        [Test]
        public void AddItemToInventoryWithFilter()
        {
            Inventory inventory = new Inventory(10);
            Predicate<ItemInstance> filter = instance => instance.Id == testItemInfoFoo.Id;
            inventory.ItemFilter = filter;
            Assert.AreEqual(inventory.ItemFilter, filter, "filter not set");


            Assert.True(inventory.AddItem(new ItemInstance(testItemInfoFoo)));
            Assert.AreEqual(1, inventory.ItemCount);

            // filter out
            Assert.False(inventory.AddItem(new ItemInstance(testItemInfoBar)));
            Assert.AreEqual(1, inventory.ItemCount);
        }

        [Test]
        public void AddItemToInventoryWithLockedAddingAndGetFalse()
        {
            Inventory inventory = new Inventory(10);
            inventory.allowAddingItems = false;

            Assert.False(inventory.AddItem(new ItemInstance(testItemInfoFoo)));
            Assert.Zero(inventory.ItemCount);
        }

        [Test]
        public void TryGetItemInfoFromInventory()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance itemInstance = new ItemInstance(testItemInfoFoo);
            inventory.AddItem(itemInstance);

            Assert.True(inventory.TryGetItem(0, out ItemInfo itemInfo1));
            Assert.AreEqual(testItemInfoFoo, itemInfo1);
            Assert.False(inventory.TryGetItem(1, out ItemInfo itemInfo2));
            Assert.IsNull(itemInfo2);
        }

        [Test]
        public void TryGetItemDataFromInventory()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance itemInstance = new ItemInstance(testItemInfoFoo);
            inventory.AddItem(itemInstance);

            Assert.True(inventory.TryGetItemData(0, out IReadOnlyDictionary<string, string> itemData1));
            Assert.AreEqual(itemInstance.ItemData, itemData1);
            Assert.False(inventory.TryGetItemData(1, out IReadOnlyDictionary<string, string> itemData2));
            Assert.IsNull(itemData2);
        }

        [Test]
        public void RemoveItemByInstanceFromInventory()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance itemInstance = new ItemInstance(testItemInfoFoo);
            inventory.AddItem(itemInstance);

            Assert.True(inventory.RemoveItem(itemInstance));
            Assert.Zero(inventory.ItemCount);
        }

        [Test]
        public void RemoveNonExitingItemByInstanceFromInventoryAndGetFalse()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance itemInstance = new ItemInstance(testItemInfoFoo);
            inventory.AddItem(itemInstance);

            Assert.True(inventory.RemoveItem(itemInstance));
            Assert.Zero(inventory.ItemCount);

            // already removed
            Assert.False(inventory.RemoveItem(itemInstance));
            Assert.Zero(inventory.ItemCount);
        }

        [Test]
        public void RemoveFirstItemByIdFromInventory()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance ii1, ii2, ii3, ii4, ii5;
            inventory.AddItem(ii1 = new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(ii2 = new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(ii3 = new ItemInstance(CreateItemInfo("z3")));
            inventory.AddItem(ii4 = new ItemInstance(CreateItemInfo("z4")));
            inventory.AddItem(ii5 = new ItemInstance(CreateItemInfo("z5")));
            Assert.AreEqual(5, inventory.ItemCount, "count after adding");

            // remove middle
            Assert.AreEqual(ii3, inventory.RemoveFirstById("z3"), "middle");
            Assert.AreEqual(4,   inventory.ItemCount, "count after middle");

            // remove first
            Assert.AreEqual(ii1, inventory.RemoveFirstById("z1"), "first");
            Assert.AreEqual(3,   inventory.ItemCount, "count after first");

            // remove last
            Assert.AreEqual(ii5, inventory.RemoveFirstById("z5"), "last");
            Assert.AreEqual(2,   inventory.ItemCount, "count after last");

            // remove only one existing
            Assert.AreEqual(ii2, inventory.RemoveFirstById("z2"), "one before last");
            Assert.AreEqual(1,   inventory.ItemCount, "count after one before last");
            Assert.AreEqual(ii4, inventory.RemoveFirstById("z4"), "only one");
            Assert.AreEqual(0,   inventory.ItemCount, "count after only one");
        }

        [Test]
        public void RemoveFirstItemByIdWhenMultipleItemsWithSameId()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance ii1A, ii1B;
            inventory.AddItem(ii1A = new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(       new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(ii1B = new ItemInstance(CreateItemInfo("z1")));
            Assert.AreEqual(3,    inventory.ItemCount);
            Assert.AreEqual(ii1A, inventory.RemoveFirstById("z1"));
            Assert.AreEqual(2,    inventory.ItemCount);
            Assert.AreEqual(ii1B, inventory.RemoveFirstById("z1"));
            Assert.AreEqual(1,    inventory.ItemCount);
        }

        [Test]
        public void RemoveFirstByIdWhenNoneExisting()
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z2")));
            Assert.AreEqual(2, inventory.ItemCount);

            Assert.Null(inventory.RemoveFirstById("z3"));
            Assert.AreEqual(2, inventory.ItemCount);
        }

        [Test]
        public void RemoveFirstByFilterFromInventory()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance ii1, ii2, ii3, ii4, ii5;
            inventory.AddItem(ii1 = new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(ii2 = new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(ii3 = new ItemInstance(CreateItemInfo("z3")));
            inventory.AddItem(ii4 = new ItemInstance(CreateItemInfo("z4")));
            inventory.AddItem(ii5 = new ItemInstance(CreateItemInfo("z5")));
            Assert.AreEqual(5, inventory.ItemCount);

            // remove middle
            Assert.AreEqual(ii3, inventory.RemoveFirstByFilter(ii => ii.Id == "z3"), "middle");
            Assert.AreEqual(4, inventory.ItemCount, "count after middle");

            // remove first
            Assert.AreEqual(ii1, inventory.RemoveFirstByFilter(ii => ii.Id == "z1"), "first");
            Assert.AreEqual(3, inventory.ItemCount, "count after first");

            // remove last
            Assert.AreEqual(ii5, inventory.RemoveFirstByFilter(ii => ii.Id == "z5"), "last");

            Assert.AreEqual(2, inventory.ItemCount, "count after last");

            // remove only one existing
            Assert.AreEqual(ii2, inventory.RemoveFirstByFilter(ii => ii.Id == "z2"), "one before last");
            Assert.AreEqual(1, inventory.ItemCount, "count after one before last");
            Assert.AreEqual(ii4, inventory.RemoveFirstByFilter(ii => ii.Id == "z4"), "only one");
            Assert.AreEqual(0, inventory.ItemCount, "count after only one");
        }

        [Test]
        public void RemoveNonExistingByFilterFromInventoryAndDoNothing()
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z2")));
            Assert.AreEqual(2, inventory.ItemCount);

            Assert.Null(inventory.RemoveFirstByFilter(ii => ii.Id == "z3"));
            Assert.AreEqual(2, inventory.ItemCount);
        }

        [Test]
        public void RemoveLatestFromInventory()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance z1, z2;

            // no items
            Assert.Null(inventory.RemoveLatest());

            // remove 2 times with 1 item
            inventory.AddItem(z1 = new ItemInstance(CreateItemInfo("z1")));
            Assert.AreEqual(z1, inventory.RemoveLatest());
            Assert.Null(inventory.RemoveLatest());
            Assert.AreEqual(0, inventory.ItemCount);

            // remove with 2 items
            inventory.AddItem(z1);
            inventory.AddItem(z2 = new ItemInstance(CreateItemInfo("z2")));
            Assert.AreEqual(z2, inventory.RemoveLatest());
            Assert.AreEqual(1, inventory.ItemCount);
        }

        [Test]
        public void RemoveWithLockedRemovingAndGetFalse()
        {
            Inventory inventory = new Inventory(10);
            inventory.allowRemovingItems = false;

            ItemInstance itemInstance = new ItemInstance(testItemInfoFoo);
            inventory.AddItem(itemInstance);

            Assert.Null(inventory.RemoveFirstById(itemInstance.Id), "by id");
            Assert.AreEqual(1, inventory.ItemCount, "by id");

            Assert.False(inventory.RemoveItem(itemInstance), "by instance");
            Assert.AreEqual(1, inventory.ItemCount, "by instance");

            Assert.Null(inventory.RemoveFirstByFilter(ii => ii.Id == testItemInfoFoo.Id), "by filter");
            Assert.AreEqual(1, inventory.ItemCount, "by filter");
        }

        [Test]
        public void HasAddedItem()
        {
            Inventory inventory = new Inventory(10);
            ItemInstance itemInstance = new ItemInstance(testItemInfoFoo);
            inventory.AddItem(itemInstance);

            Assert.True(inventory.HasItemById(testItemInfoFoo.Id));
            Assert.True(inventory.HasItemByFilter(ii => ii.Id == testItemInfoFoo.Id));

            Assert.False(inventory.HasItemById("non-existing"));
            Assert.False(inventory.HasItemByFilter(ii => ii.Id == "non-existing"));
        }

        [Test]
        public void CountsAddedItemsById()
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z3")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            Assert.AreEqual(3, inventory.CountItemsById("z1"), "z1");
            Assert.AreEqual(1, inventory.CountItemsById("z2"), "z2");
            Assert.AreEqual(1, inventory.CountItemsById("z3"), "z3");
            Assert.AreEqual(0, inventory.CountItemsById("non-existing"), "non-existing");
        }

        [Test]
        public void ProcessItems()
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));

            Assert.False(inventory.GetItemData(0).ContainsKey("processed"), "init key");
            Assert.False(inventory.GetItemData(1).ContainsKey("processed"), "init key");
            Assert.False(inventory.GetItemData(2).ContainsKey("processed"), "init key");
            int callCount = 0;

            int processCount = inventory.ProcessAllItemsWithId("z1", ii =>
            {
                ii.ItemData["processed"] = "ok";
                callCount++;
            });
            Assert.AreEqual(2, callCount, "call count");
            Assert.AreEqual(2, processCount, "process count");
            Assert.AreEqual("ok", inventory.GetItemData(0)["processed"], "z1 processed");
            Assert.AreEqual("ok", inventory.GetItemData(2)["processed"], "z1 processed");

            Assert.False(inventory.GetItemData(1).ContainsKey("processed"), "z2 not processed");

            Assert.AreEqual(3, inventory.ItemCount, "item count should not change");
            Assert.AreEqual(2, inventory.CountItemsById("z1"), "z1 count should be the same");
            Assert.AreEqual(1, inventory.CountItemsById("z2"), "z2 count should be the same");
        }

        [Test]
        public void ProcessFirstItem()
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));

            Assert.False(inventory.GetItemData(0).ContainsKey("processed"), "init key");
            Assert.False(inventory.GetItemData(1).ContainsKey("processed"), "init key");
            Assert.False(inventory.GetItemData(2).ContainsKey("processed"), "init key");
            int callCount = 0;
            bool processResult = inventory.ProcessFirstItemWithId("z1", ii =>
            {
                ii.ItemData["processed"] = "ok";
                callCount++;
            });
            Assert.AreEqual(1, callCount, "call count");
            Assert.True(processResult, "process result");
            Assert.AreEqual("ok", inventory.GetItemData(0)["processed"], "first z1 processed");

            Assert.False(inventory.GetItemData(1).ContainsKey("processed"), "z2 not processed");
            Assert.False(inventory.GetItemData(2).ContainsKey("processed"), "second z1 not processed");

            Assert.AreEqual(3, inventory.ItemCount, "item count should not change");
            Assert.AreEqual(2, inventory.CountItemsById("z1"), "z1 count should be the same");
            Assert.AreEqual(1, inventory.CountItemsById("z2"), "z2 count should be the same");
        }

        [Test]
        public void ProcessNonExistingItemAndDoNothing()
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            Assert.False(inventory.GetItemData(0).ContainsKey("processed"), "init key");

            int callCount = 0;
            bool processResult = inventory.ProcessFirstItemWithId("non-existing", ii =>
            {
                ii.ItemData["processed"] = "ok";
                callCount++;
            });
            Assert.AreEqual(0, callCount, "call count");
            Assert.False(processResult, "processResult");

            Assert.False(inventory.GetItemData(0).ContainsKey("processed"), "z1 not processed");

            Assert.AreEqual(1, inventory.ItemCount, "item count should not change");
        }

        [Test]
        public void FireEvents()
        {
            Inventory inventory = new Inventory(50);
            // leave some space for "AddItem" to work
            for (int i = 0; i < inventory.Capacity / 2; i++) inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            ItemInstance itemInstance = new ItemInstance(CreateItemInfo("z1"));
            bool added = false;
            bool removed = false;
            bool processed = false;
            inventory.OnItemAdded += (_, _) => added = true;
            inventory.OnItemRemoved += (_, _) => removed = true;
            inventory.OnItemProcessed += (_, _) => processed = true;

            Assert.False(added, "initial added");
            Assert.False(removed, "initial removed");
            Assert.False(processed, "initial processed");
            inventory.AddItem(itemInstance);
            Assert.True(added, "addItem added");
            Assert.False(removed, "addItem removed");
            Assert.False(processed, "addItem processed");

            added = false;
            removed = false;
            inventory.RemoveItem(itemInstance);
            Assert.False(added, "removeItem added");
            Assert.True(removed, "removeItem removed");
            Assert.False(processed, "removeItem processed");

            removed = false;
            inventory.RemoveFirstById(itemInstance.Id);
            Assert.False(added, "removeFirstById added");
            Assert.True(removed, "removeFirstById removed");
            Assert.False(processed, "removeFirstById processed");

            removed = false;
            inventory.RemoveFirstByFilter(ii => ii.Id == itemInstance.Id);
            Assert.False(added, "removeFirstByFilter added");
            Assert.True(removed, "removeFirstByFilter removed");
            Assert.False(processed, "removeFirstByFilter processed");

            removed = false;
            processed = false;
            inventory.ProcessFirstItemWithId(itemInstance.Id, _ => { });
            Assert.False(added, "processFirstItemWithId added");
            Assert.False(removed, "processFirstItemWithId removed");
            Assert.True(processed, "processFirstItemWithId processed");

            int processCount = 0;
            inventory.OnItemProcessed += (_, _) => processCount++;

            processed = false;
            inventory.ProcessAllItemsWithId(itemInstance.Id, _ => { });
            Assert.False(added, "processAllItemsWithId added");
            Assert.False(removed, "processAllItemsWithId removed");
            Assert.True(processed, "processAllItemsWithId processed");
            Assert.AreEqual(inventory.CountItemsById(itemInstance.Id), processCount);
        }
    }
}
