using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    [System.Serializable]
    public struct Route
    {
        public Vector3 waypoint;
        public bool release;
        public bool grab;
    }
}
