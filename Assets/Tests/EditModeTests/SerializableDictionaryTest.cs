using System.Collections.Generic;
using Cosmobot.Utils;
using NUnit.Framework;

namespace Cosmobot
{
    public class SerializableDictionaryTest
    {

        [Test]
        public void AddPairsAndKeepKeyValueOrder()
        {
            SerializableDictionary<string, string> dict = new();
            Assert.IsEmpty(dict.Keys);
            Assert.IsEmpty(dict.Values);

            dict.Add("key1", "value1");
            dict.Add("key2", "value2");
            dict["key3"] = "value3";

            Assert.That(dict.Keys, Is.EquivalentTo(new[] { "key1", "key2", "key3" }));
            Assert.That(dict.Values, Is.EquivalentTo(new[] { "value1", "value2", "value3" }));
        }

        [Test]
        public void RemoveKey()
        {
            SerializableDictionary<string, string> dict = new()
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };

            dict.Remove("key2");

            Assert.That(dict.Keys, Is.EquivalentTo(new[] { "key1", "key3" }));
            Assert.That(dict.Values, Is.EquivalentTo(new[] { "value1", "value3" }));

            Assert.False(dict.Remove("xxxx"));

            Assert.True(dict.Remove("key1"));
            Assert.True(dict.Remove("key3"));
            Assert.False(dict.Remove("key3"));
        }

        [Test]
        public void ReadValues()
        {
            SerializableDictionary<string, string> dict = new()
            {
                { "key1", "value1" },
                { "key2", "value2" },
                { "key3", "value3" }
            };
            Assert.AreEqual(dict["key1"], "value1");
            Assert.AreEqual(dict["key2"], "value2");
            Assert.AreEqual(dict["key3"], "value3");

            Assert.Throws<KeyNotFoundException>(() => dict.GetValue("xxxx"));

            var enumerator = dict.GetEnumerator();
            enumerator.MoveNext();
            Assert.AreEqual(enumerator.Current, new KeyValuePair<string, string>("key1", "value1"));
            enumerator.MoveNext();
            Assert.AreEqual(enumerator.Current, new KeyValuePair<string, string>("key2", "value2"));
            enumerator.MoveNext();
            Assert.AreEqual(enumerator.Current, new KeyValuePair<string, string>("key3", "value3"));
            Assert.False(enumerator.MoveNext());
        }

    }
}
