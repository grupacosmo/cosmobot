using System;

namespace Cosmobot
{
    /// <summary>
    /// Allows to access values in a <c>SerializableDictionary&ltstring, string&gt</c> with a specified type by using
    /// [] operator. Values are converted to the specified type when getting them and to string when setting them.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueAccessor<T>
    {
        private readonly SerializableDictionary<string, string> array;

        public ValueAccessor(SerializableDictionary<string, string> array)
        {
            this.array = array;
        }

        public T this[string index]
        {
            get => (T)Convert.ChangeType(array[index], typeof(T));
            set => array[index] = ValueToString(value);
        }

        private string ValueToString(T value)
        {
            return SerializationUtils.ToString(value);
        }
    }
}