using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cosmobot
{
    public class RobotRoutingTool : MonoBehaviour
    {
        public bool On;
        public GameObject currentRobot;
        public Route[] currentRoute;
        public int currentRouteIndex = 0;

        public bool shouldGrab;
        public bool shouldRelease;
        public bool addWaypoint(Vector3 waypoint)
        {
            var length = currentRoute.Length;
            currentRoute[length].waypoint = waypoint;
            currentRoute[length].grab = shouldGrab;
            currentRoute[length].release = shouldRelease;

            return true;
        }
        void Start()
        {
        
        }

        public void

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
