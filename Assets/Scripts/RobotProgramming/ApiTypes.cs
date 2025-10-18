using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot.Api.Types
{
    public class Vec3
    {
        public float x, y, z;

        public Vec3()
        {
            this.x = 0.0f;
            this.y = 0.0f;
            this.z = 0.0f;
        }

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator Vec3(Vector3 v)
        {
            return new Vec3(v.x, v.y, v.z);
        }

        public static implicit operator Vector3(Vec3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator Vec3(Vec2 v)
        {
            return new Vec3(v.x, 0, v.y);
        }
    }

    public class Vec2
    {
        public float x, y;

        public Vec2()
        {
            this.x = 0.0f;
            this.y = 0.0f;
        }

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vec2(Vector2 v)
        {
            return new Vec2(v.x, v.y);
        }

        public static implicit operator Vec2(Vector3 v)
        {
            return new Vec2(v.x, v.z);
        }

        public static implicit operator Vector2(Vec2 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator Vector3(Vec2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }
    }
}
namespace Cosmobot.Api.TypesInternal
{
    using System;
    using Cosmobot.Api.Types;
    public abstract class Entity
    {
        internal Transform transform;
        internal Func<bool> IsValidHandler;
        internal Func<Vector2> positionHandler;
        internal Entity(Component component, ProgrammableFunctionWrapper wrapper)
        {
            transform = component.transform;
            positionHandler = wrapper.WrapOneFrame<Vector2>(() => component.transform.position);
            IsValidHandler = wrapper.WrapOneFrame<bool>(() => component);
        }

        public bool isValid()
        {
            return IsValidHandler();
        }

        public Vec2 getPosition()
        {
            if (!isValid())
            {
                Debug.LogError("Couldn't get position");
                return null;
            }
            return positionHandler();
        }
    }

    public class Item : Entity
    {
        internal ItemComponent itemComponent; // Jint shouldn't have access to Unity types

        public Item(ItemComponent itemComponent, ProgrammableFunctionWrapper wrapper) : base(itemComponent, wrapper)
        {
            this.itemComponent = itemComponent;
        }
    }

    public class Hostile : Entity
    {
        internal Enemy enemyComponent;  // Jint shouldn't have access to Unity types

        public Hostile(Enemy enemyComponent, ProgrammableFunctionWrapper wrapper) : base(enemyComponent, wrapper)
        {
            this.enemyComponent = enemyComponent;
        }
    }
}
