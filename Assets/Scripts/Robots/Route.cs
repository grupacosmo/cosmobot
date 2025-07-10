using System;
using UnityEngine;

namespace Cosmobot
{
    [Serializable]
    public struct Route
    {
        public Vector3 waypoint;

        public bool grab;
        public bool release;
    }
}
