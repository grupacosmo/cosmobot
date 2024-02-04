using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Cosmobot.ItemSystem
{

    public class ItemsTest
    {
        private const string TestProperty = "TestProperty";
        private const string TestItemId = "test-item";
        private const string ItemTestSceneDir = "Assets/Tests/PlayModeTests/Utils/Items/ItemTestScene.unity";

        [OneTimeSetUp]
        public void SetUp()
        {
            EditorSceneManager.LoadSceneInPlayMode(ItemTestSceneDir, new LoadSceneParameters(LoadSceneMode.Single));
        }

        [UnityTest]
        public IEnumerator ShouldAcquireItemsFromManager()
        {
            IReadOnlyList<ItemInfo> items = ItemManager.Instance.Items;

            Assert.AreEqual(items.Count, 1, "Should have 1 item in the list");
            ItemInfo itemInfo = items[0];
            Assert.NotNull(ItemManager.Instance.GetItem(itemInfo.Id));

            Assert.AreEqual(itemInfo.Id, TestItemId);
            Assert.AreEqual(itemInfo.DisplayName, "Test Item");
            Assert.AreEqual(itemInfo.AdditionalData.Count, 1);
            Assert.AreEqual(itemInfo.AdditionalData[TestProperty], "TestValue");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShouldInstantiateIndependentObject()
        {
            ItemInfo itemInfo = ItemManager.Instance.GetItem(TestItemId);
            GameObject object1 = itemInfo.InstantiateItem(Vector3.zero, Quaternion.identity);
            GameObject object2 = itemInfo.InstantiateItem(Vector3.zero, Quaternion.identity);
            Assert.AreNotEqual(object1, object2);
            Item item1 = object1.GetComponent<Item>();
            Item item2 = object2.GetComponent<Item>();
            Assert.AreEqual(item1.ItemData[TestProperty], "TestValue");
            Assert.AreEqual(item2.ItemData[TestProperty], "TestValue");
            item1.ItemData[TestProperty] = "NewValue";
            Assert.AreEqual(item1.ItemData[TestProperty], "NewValue");
            Assert.AreEqual(item2.ItemData[TestProperty], "TestValue");
            Assert.AreEqual(itemInfo.AdditionalData[TestProperty], "TestValue");

            GameObject.Destroy(object1);
            GameObject.Destroy(object2);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShouldAccessDataViaAccessors()
        {
            const string nonExistingKey = "nonExistingKey";
            const string nullableOpKey = "nullableOpKey";
            const string newKeyOne = "newKeyOne";
            const string newKeyTwo = "newKeyTwo";

            ItemInfo itemInfo = ItemManager.Instance.GetItem(TestItemId);
            GameObject object1 = itemInfo.InstantiateItem(Vector3.zero, Quaternion.identity);
            Item item = object1.GetComponent<Item>();

            // operator [] not found exceptions
            Assert.Throws<KeyNotFoundException>(() => { bool x = item.BoolValue[nonExistingKey]; });

            // math on nullable
            item.SetIntValue(nullableOpKey, item.GetIntValue(nonExistingKey) + 2);
            Assert.IsNull(item.GetIntValue(nullableOpKey));

            // conversion
            item.SetFloatValue(newKeyOne, 3.14f);
            Assert.AreEqual(3.14f, item.GetFloatValue(newKeyOne));
            Assert.AreEqual(3.14f, item.FloatValue[newKeyOne]);
            Assert.AreEqual("3.14", item.StringValue[newKeyOne]);
            Assert.IsNull(item.GetIntValue(newKeyOne));
            Assert.Throws<FormatException>(() => { int x = item.IntValue[newKeyOne]; });

            item.BoolValue[newKeyTwo] = true;
            Assert.AreEqual(true, item.BoolValue[newKeyTwo]);
            Assert.AreEqual("true", item.StringValue[newKeyTwo]);
            Assert.IsNull(item.GetIntValue(newKeyTwo));
            Assert.IsNull(item.GetFloatValue(newKeyTwo));
            Assert.Throws<FormatException>(() => { int x = item.IntValue[newKeyTwo]; });
            Assert.Throws<FormatException>(() => { float x = item.FloatValue[newKeyTwo]; });

            GameObject.Destroy(object1);
            yield return null;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            EditorWindow.FocusWindowIfItsOpen<TestRunnerWindow>();
        }
    }

}