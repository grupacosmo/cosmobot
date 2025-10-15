using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot.Api.Types
{
    public struct vec3
    {
        public float x, y, z;

        public vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator vec3(Vector3 v)
        {
            return new vec3(v.x, v.y, v.z);
        }

        public static implicit operator Vector3(vec3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator vec3(vec2 v)
        {
            return new vec3(v.x, 0, v.y);
        }
    }

    public struct vec2
    {
        public float x, y;

        public vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator vec2(Vector2 v)
        {
            return new vec2(v.x, v.y);
        }

        public static implicit operator vec2(Vector3 v)
        {
            return new vec2(v.x, v.z);
        }

        public static implicit operator Vector2(vec2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator Vector3(vec2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }
    }
}
namespace Cosmobot.Api.TypesInternal
{
    using Cosmobot.Api.Types;
    public abstract class Entity
    {
        public vec2 position { get; set; }
        internal virtual bool IsValid { get; } // This is needed to know if Entity is still there
    }

    public class Item : Entity
    {
        internal ItemComponent itemComponent; // Jint shouldn't have access to Unity types
        internal override bool IsValid => itemComponent != null;

        public Item(ItemComponent itemComponent, vec2 position)
        {
            this.itemComponent = itemComponent;
            this.position = position;
        }
    }

    public class Hostile : Entity
    {
        internal Enemy enemyComponent;  // Jint shouldn't have access to Unity types
        internal override bool IsValid => enemyComponent != null;

        public Hostile(Enemy enemyComponent, vec2 position)
        {
            this.enemyComponent = enemyComponent;
            this.position = position;
        }
    }
}
