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

    [System.Serializable]
    public class RouteArray
    {
        public Route[] route;

        public RouteArray(int size)
        {
            route = new Route[size];
        }

        public void AddWaypoint()
        {
            Route[] newArray = new Route[route.Length + 1];
            for (int i = 0; i < route.Length; i++)
            {
                newArray[i] = route[i];
            }
            newArray[newArray.Length - 1] = new Route();
            route = newArray;
        }

        public void RemoveWaypoint(int index)
        {
            if (index >= 0 && index < route.Length)
            {
                Route[] newArray = new Route[route.Length - 1];
                for (int i = 0, j = 0; i < route.Length; i++)
                {
                    if (i != index)
                    {
                        newArray[j] = route[i];
                        j++;
                    }
                }
                route = newArray;
            }
        }
    }
}
