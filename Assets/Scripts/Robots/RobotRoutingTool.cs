using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Cosmobot
{
    public class RobotRoutingTool : MonoBehaviour
    {
        [Header("HOW TO USE:\nF - switch to new robot\nLeft MB - add waypoint\nRight MB - set route\nF1/F2 - swap grab/release modes\n")]
        public bool isOn;
        public float maxPointDistance = 20f;
        public GameObject currentRobot = null;

        [Header("Waypoint Modes")]
        public bool resetModesAtWaypoints;
        public bool grabAtWaypoint;
        public bool releaseAtWaypoint;

        [Header("Route")]
        public List<Route> currentRoute = new List<Route>();
        public LayerMask floorLayer;
        public string robotTag;

        [Header("Route Preview")]
        public bool clearRoutePreviewAfterApplying;
        private LineRenderer lineRenderer;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void DrawLine()
        {
            RouteMovement routeMovement = currentRobot.GetComponentInChildren<RouteMovement>();

            if (routeMovement.loopMode) lineRenderer.loop = true;
            else lineRenderer.loop = false;

            for (int i = 0; i < currentRoute.Count; i++)
            {
                Vector3 offsetY = new Vector3(0f, 0.1f, 0f);
                lineRenderer.SetPosition(i, currentRoute[i].waypoint + offsetY);
            }
        }
        private void addWaypoint(Vector3 waypoint)
        {
            var length = currentRoute.Count;
            Route newWaypoint = new Route();

            newWaypoint.waypoint = waypoint;
            newWaypoint.grab = grabAtWaypoint;
            newWaypoint.release = releaseAtWaypoint;

            currentRoute.Add(newWaypoint);

            if (resetModesAtWaypoints)
            {
                grabAtWaypoint = false;
                releaseAtWaypoint = false;
            }

            lineRenderer.enabled = true;
            lineRenderer.positionCount = currentRoute.Count;
            DrawLine();
        }
        private Vector3 GetLookedAtPointCoordinates()
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxPointDistance, floorLayer))
            {
                return hit.point;
            }
            else return Vector3.zero;
        }
        public void AddLookedAtWaypoint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (currentRobot == null) return;

                var waypoint = GetLookedAtPointCoordinates();
                if (waypoint != Vector3.zero) addWaypoint(waypoint);
            }
        }
        public void SwitchToLookedAtRobot(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                RaycastHit hit;

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag(robotTag))
                    {
                        Debug.Log("Switched to new robot: " + hit.collider.gameObject.name);
                        currentRobot = hit.collider.gameObject;
                    }
                }
            }
        }
        public void ApplyRoute(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (currentRobot is null) return;

                RouteMovement routeMovement = currentRobot.GetComponentInChildren<RouteMovement>();
                routeMovement.route = new List<Route>(currentRoute);

                currentRoute.Clear();

                if (clearRoutePreviewAfterApplying) lineRenderer.enabled = false;
            }
        }
        public void ChangeReleasedMode(InputAction.CallbackContext context)
        {
            if (context.performed) releaseAtWaypoint = !releaseAtWaypoint;
        }
        public void ChangeGrabMode(InputAction.CallbackContext context)
        {
            if (context.performed) grabAtWaypoint = !grabAtWaypoint;
        }
    }
}
