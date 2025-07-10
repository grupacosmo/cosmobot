using UnityEngine;

namespace Cosmobot.Entity
{
    public struct DamageSource
    {
        public static readonly DamageSource Empty = new DamageSource(null, "null");
        public Object Source { get; }
        public string Name { get; private set; }

        public bool IsEmpty => Source == null;

        public DamageSource(Object source)
        {
            Source = source;
            Name = source.name;
        }

        public DamageSource(Object source, string name)
        {
            Source = source;
            Name = name;
        }
    }
}
