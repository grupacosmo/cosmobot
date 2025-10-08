using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot.Api.Types
{
    public struct Vec3
    {
        public float x, y, z;

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

    public struct Vec2
    {
        public float x, y;

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

    public interface Entity
    {
        Vec2 position { get; }
    }

    public struct Item : Entity
    {
        public ItemComponent itemComponent;
        public Vec2 position { get; }

        public Item(ItemComponent itemComponent, Vec2 position)
        {
            this.itemComponent = itemComponent;
            this.position = position;
        }
    }
}
