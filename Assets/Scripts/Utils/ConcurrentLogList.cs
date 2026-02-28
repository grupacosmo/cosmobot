using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cosmobot.Utils
{
    public class ConcurrentLogList<T> : IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
    {
        private const int GrowFactor = 2;
        public const int DefaultCapacity = 20;
        
        public int Capacity { get; private set; }
        public int Count { get; private set; }
        public bool IsSynchronized => true;
        public object SyncRoot => lck;
        
        public bool IsFixedSize => false;
        public bool IsReadOnly => false;

        private object lck = new object();

        private T[] array;
        
        public ConcurrentLogList() : this(DefaultCapacity) { }

        public ConcurrentLogList(int capacity)
        {
            Capacity = capacity;
            Count = 0;
            array = new T[capacity];
        }
        
        public T this[int index]
        {
            get => At(index);
            set {
                throw new InvalidOperationException("Cannot change existing value. This collection allows only Adding to the end");    
            }
        }

        public void Add(T value)
        {
            lock (lck)
            {
                if (Count >= Capacity)
                {
                    EnsureCapacity(GetNewCapacity());
                    if (Capacity <= Count) throw new OutOfMemoryException("Cannot ensure enough capacity to add new element");
                }
                array[Count] = value;
                Count++;
            }
        }

        private int GetNewCapacity()
        {
            if (Capacity >= int.MaxValue / GrowFactor)
            {
                return int.MaxValue;
            }
            
            return Capacity == 0 ? DefaultCapacity : Capacity * 2;
        }
        
        private void EnsureCapacity(int newCapacity)
        {
            lock (lck)
            {
                System.Diagnostics.Debug.Assert(newCapacity > 0);
                if (newCapacity < Capacity) return;
                if (Capacity < newCapacity)
                {
                    T[] newArray = new T[newCapacity];
                    Array.Copy(array, newArray, Count);
                    array = newArray;
                    Capacity = newCapacity;
                }
            }
        }
        
        public void Clear()
        {
            lock (lck)
            {
                if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
                {
                    Array.Clear(array, 0, Count);
                    Count = 0;
                }
                else    
                {
                    Count = 0;    
                }
            }
        }

        /// <summary>
        /// Same as <see cref="List{T}.Contains"/> but will check only part of array that exist at the moment of call.
        /// Any elements added during function call will be ignored. It is better to first copy array and then work on
        /// copy. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value)
        {
            return IndexOf(value) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int currentSize = Count;
            T[] currentArray = this.array;
            for (int i = arrayIndex; i < currentSize && i < array.Length; i++)
            {
                array[i] = currentArray[i];
            }
        }
        
        public void CopyTo(Array array, int arrayIndex)
        {
            int currentSize = Count;
            T[] currentArray = this.array;
            for (int i = arrayIndex; i < currentSize && i < array.Length; i++)
            {
                array.SetValue(currentArray, i);
            }
        }
        
        /// <summary>
        /// Same as <see cref="List{T}.IndexOf(T)"/> but will check only part of array that exist at the moment of call.
        /// Any elements added during function call will be ignored. It is better to first copy array and then work on
        /// copy. 
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The zero-based index of the first occurrence of item, if found; otherwise, -1</returns>
        public int IndexOf(T value)
        {
            int currentCount = Count;
            T[] currentArray = array;
            for (int i = 0; i < currentCount; i++)
            {
                bool bothNull = currentArray[i] == null && value == null;
                if (bothNull || (currentArray[i]?.Equals(value) ?? false))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, T value)
        {
            throw new InvalidOperationException("This collection allows only Adding to the end. Use Add(T)");    
        }

        public bool Remove(T value)
        {
            throw new InvalidOperationException("This collection does not allow to Remove");
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException("This collection does not allow to Remove");
        }

        public T At(int index)
        {
            if (index < 0 || index >= Count) throw new IndexOutOfRangeException();
            return array[index];
        }

        public IEnumerator<T> GetEnumerator()
        {
            int currentCount = Count;
            T[] currentArray = array;
            for (int i = 0; i < currentCount; i++)
            {
                yield return currentArray[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
