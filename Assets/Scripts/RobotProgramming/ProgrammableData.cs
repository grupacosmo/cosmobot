using System;

namespace Cosmobot
{
    /// <summary>
    /// Represents instance of programmable devices (eg. robots).
    ///
    /// Used by other components to access programmable device data.
    /// It is thread safe, and it is mostly safe to use outside Unity's Main Thread <b>but some fields returns Unity
    /// Object references which should only be used inside Unity's Main Thread</b>. Those "unsafe" fields are tagged as
    /// "Unity" fields and additionally provided with warning in documentation.  
    ///
    /// Use <see cref="Programmable"/> as Unity Component for GameObjects.
    ///
    /// Instance of this class should be only created by <see cref="Programmable"/>.
    ///  
    /// </summary>
    public class ProgrammableData : IEquatable<ProgrammableData>
    {
        private readonly Programmable parent;
        public readonly int InstanceID;
        public readonly string Name;
        // TODO: implement thread safe:
        //  - safe "isRunning" - is robot JS Engine running 
        //  - safe "isValid" - is parent != null 
        //  both probably need implementation in ProgrammableComponent
        //  eg. using Interlocked.Exchange/Read or sth on variable that will be set OnDisable/Enable and Engine finally 

        /// <summary>All content of this field is bind to Unit's main thread. Use only from Unity's main thread</summary> 
        public readonly UnityReference Unity;

        public ProgrammableData(Programmable parent)
        {
            this.parent = parent;

            // not sure about this, GameObject name can change over time, but it would be nice to keep this API simple
            // without need to thread sync every read operation
            Name = parent.name;
            InstanceID = parent.GetInstanceID();
            Unity = new UnityReference(this);
        }

        // just "folder" for creating nice looking api 
        public class UnityReference
        {
            private readonly ProgrammableData data; // java does this better
            public Programmable ProgrammableComponent => data.parent;

            public UnityReference(ProgrammableData parent)
            {
                data = parent;
            }
        }

        public bool Equals(ProgrammableData other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return InstanceID == other.InstanceID;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProgrammableData)obj);
        }

        public override int GetHashCode()
        {
            return InstanceID;
        }
    }
}
