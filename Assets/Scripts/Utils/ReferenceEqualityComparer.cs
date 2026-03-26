using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cosmobot.Utils
{
    public class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        public new bool Equals(T x, T y)
        {
            return System.Object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
