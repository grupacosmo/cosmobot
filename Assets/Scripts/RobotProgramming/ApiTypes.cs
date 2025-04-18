using UnityEngine;

namespace Cosmobot.Api.Types
{
    public struct Vec3
    {
        float x, y, z;

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
    }

    public struct Vec2
    {
        float x, y;

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator Vec2(Vector2 v)
        {
            return new Vec2(v.x, v.y);
        }

        public static implicit operator Vector2(Vec2 v)
        {
            return new Vector2(v.x, v.y);
        }
    }
}
