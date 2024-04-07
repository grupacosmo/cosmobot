using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Events;
using UnityEditor.Callbacks;

namespace Cosmobot
{
    public class RouteMovement : MonoBehaviour
    {
        public float robotSpeed;
        public float gravity = 50;
        public float distanceToGround = 0.1f;
        public Vector3 direction = Vector3.zero;

        #region route
        public bool loopMode;
        public int routeIndex = 0;
        public Route[] route;
        private bool routeForward = true;
        #endregion route

        private Rigidbody rb;
        private Grabber grabber;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            grabber = GetComponentInChildren<Grabber>();
        }

        void FixedUpdate()
        {
            ProcessMovement();
            ProcessRotation();
        }

        private void ProcessMovement()
        {
            if (CheckIfArrived(route[routeIndex].waypoint)) 
            {
                UpdateRoute();
                if (grabber != null) GrabOrRelease();
            }
            
            Vector3 directionToPoint = GetDirectionToPoint(transform.position, route[routeIndex].waypoint);
            directionToPoint.y = 0f;

            if (IsGrounded())
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            }
            else
            {
                directionToPoint += Vector3.down * gravity * Time.fixedDeltaTime;
            }
            directionToPoint.Normalize();

            Vector3 velocity = directionToPoint * robotSpeed;
            rb.velocity = velocity;
        }
        private void ProcessRotation()
        {
            var directionToPoint = GetDirectionToPoint(transform.position, route[routeIndex].waypoint);
            var rotation = Quaternion.LookRotation(directionToPoint);

            direction = directionToPoint;

            transform.rotation = rotation;
        
        }
        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, distanceToGround + 0.1f);
        }

        Vector3 GetDirectionToPoint(Vector3 pointFrom, Vector3 pointTo)
        {
            Vector3 directionToPoint = pointTo - pointFrom;
            directionToPoint = new Vector3(directionToPoint.x, 0, directionToPoint.z).normalized;

            return directionToPoint;
        }

        private void UpdateRoute()
        {
            if (routeForward == true)
            {
                if (routeIndex == route.Length - 1) 
                {
                    if (loopMode) routeIndex = -1;
                    else routeForward = false;
                }
            } 
            else if (routeIndex == 0) routeForward = true;

            int add_amount = routeForward ? 1 : -1;
            routeIndex += add_amount;
        }
        private void GrabOrRelease()
        {
            if (grabber.automaticGrabMode) return;
            else if (route[routeIndex].release && (grabber.grabbedItem != null))
            {
                grabber.ReleaseItem();
            }
            else if (route[routeIndex].grab && (grabber.grabbedItem == null))
            {
                grabber.GrabItem();
            }
            
        }
        private bool CheckIfArrived(Vector3 point)
        {
            var currentPosition = new Vector3(transform.position.x, 0f, transform.position.z);
            point = new Vector3(point.x, 0f, point.z);

            float distanceToTarget = Vector3.Distance(currentPosition, point);
            float arrivalThreshold = 0.1f;

            if (distanceToTarget <= arrivalThreshold)
            {
                Debug.Log($"Robot has arrived at the point number '{routeIndex}'.");
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }
}
