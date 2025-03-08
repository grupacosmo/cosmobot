using Cosmobot.ItemSystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Cosmobot
{
    public class InventoryTest
    {
        private ItemInfo testItemInfoFoo;
        private ItemInfo testItemInfoBar;
        
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
            inventory.ItemFilter = instance => instance.Id == testItemInfoFoo.Id; 
            
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
            Assert.AreEqual(5, inventory.ItemCount);
            
            // remove middle
            Assert.AreEqual(ii3, inventory.RemoveFirstById("z3"));
            Assert.AreEqual(4,   inventory.ItemCount);
            
            // remove first
            Assert.AreEqual(ii1, inventory.RemoveFirstById("z1"));
            Assert.AreEqual(3,   inventory.ItemCount);
            
            // remove last
            Assert.AreEqual(ii5, inventory.RemoveFirstById("z5"));
            Assert.AreEqual(2,   inventory.ItemCount);
            
            // remove only one existing
            Assert.AreEqual(ii2, inventory.RemoveFirstById("z2"));
            Assert.AreEqual(1,   inventory.ItemCount);
            Assert.AreEqual(ii4, inventory.RemoveFirstById("z4"));
            Assert.AreEqual(0,   inventory.ItemCount);
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
        
        public void RemoveFirstByFilrerFromInventory()
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
            Assert.AreEqual(ii3, inventory.RemoveFirstByFilter((ii)=>ii.Id == "z3"));
            Assert.AreEqual(4, inventory.ItemCount);
            
            // remove first
            Assert.AreEqual(ii1, inventory.RemoveFirstByFilter((ii)=>ii.Id == "z1"));
            Assert.AreEqual(3, inventory.ItemCount);
            
            // remove last
            Assert.AreEqual(ii5, inventory.RemoveFirstByFilter((ii)=>ii.Id == "z5"));
            Assert.AreEqual(2, inventory.ItemCount);
            
            // remove only one existing
            Assert.AreEqual(ii2, inventory.RemoveFirstByFilter((ii)=>ii.Id == "z2"));
            Assert.AreEqual(1, inventory.ItemCount);
            Assert.AreEqual(ii4, inventory.RemoveFirstByFilter((ii)=>ii.Id == "z4"));
            Assert.AreEqual(0, inventory.ItemCount);
        }
        
        [Test]
        public void RemoveWithLockedRemovingAndGetFalse()
        {
            Inventory inventory = new Inventory(10);
            inventory.allowRemovingItems = false;
            
            ItemInstance itemInstance = new ItemInstance(testItemInfoFoo);
            inventory.AddItem(itemInstance);
            
            Assert.Null(inventory.RemoveFirstById(itemInstance.Id));
            Assert.AreEqual(1, inventory.ItemCount);
            
            Assert.False(inventory.RemoveItem(itemInstance));
            Assert.AreEqual(1, inventory.ItemCount);
            
            Assert.Null(inventory.RemoveFirstByFilter((ii)=>ii.Id == testItemInfoFoo.Id));
            Assert.AreEqual(1, inventory.ItemCount);
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
            Assert.AreEqual(3, inventory.CountItemsById("z1"));
            Assert.AreEqual(1, inventory.CountItemsById("z2"));
            Assert.AreEqual(1, inventory.CountItemsById("z3"));
            Assert.AreEqual(0, inventory.CountItemsById("non-existing"));
        }
        
        [Test]
        public void ProcessItems() 
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            
            Assert.False(inventory.GetItemData(0).ContainsKey("processed"));
            Assert.False(inventory.GetItemData(1).ContainsKey("processed"));
            Assert.False(inventory.GetItemData(2).ContainsKey("processed"));
            int callCount = 0;
            
            int processCount = inventory.ProcessAllItemsWithId("z1", ii =>
            {
                ii.ItemData["processed"] = "ok";
                callCount++;
            });
            Assert.AreEqual(2, callCount);
            Assert.AreEqual(2, processCount);
            Assert.AreEqual("ok", inventory.GetItemData(0)["processed"]);
            Assert.AreEqual("ok", inventory.GetItemData(2)["processed"]);
            
            Assert.False(inventory.GetItemData(1).ContainsKey("processed"));
            
            Assert.AreEqual(3, inventory.ItemCount);
            Assert.AreEqual(2, inventory.CountItemsById("z1"));
            Assert.AreEqual(1, inventory.CountItemsById("z2"));
        }
        
        [Test]
        public void ProcessFirstItem()
        {
            Inventory inventory = new Inventory(10);
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z2")));
            inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            
            Assert.False(inventory.GetItemData(0).ContainsKey("processed"));
            Assert.False(inventory.GetItemData(1).ContainsKey("processed"));
            Assert.False(inventory.GetItemData(2).ContainsKey("processed"));
            int callCount = 0;
            bool processResult = inventory.ProcessFirstItemWithId("z1", ii =>
            {
                ii.ItemData["processed"] = "ok";
                callCount++;
            });
            Assert.AreEqual(1, callCount);
            Assert.True(processResult);
            Assert.AreEqual("ok", inventory.GetItemData(0)["processed"]);
            
            Assert.False(inventory.GetItemData(1).ContainsKey("processed"));
            Assert.False(inventory.GetItemData(2).ContainsKey("processed"));
            
            Assert.AreEqual(3, inventory.ItemCount);
            Assert.AreEqual(2, inventory.CountItemsById("z1"));
            Assert.AreEqual(1, inventory.CountItemsById("z2"));
        }
        
        [Test]
        public void FireEvents() 
        {
            Inventory inventory = new Inventory(50);
            // leave some space for "AddItem" to work
            for(int i = 0 ; i < inventory.Capacity / 2; i++)
            {
                inventory.AddItem(new ItemInstance(CreateItemInfo("z1")));
            }
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