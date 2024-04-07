using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.Events;

namespace Cosmobot
{
    public class MoveOnRoute : MonoBehaviour
    {
        public bool canMove;
        public bool CanMove
        {
            get {return canMove;}
            set
            {
                canMove = value;
 
                if (!value)
                {
                    Direction = Vector3.zero;
                    rb.velocity = Vector3.zero;
                }
            }
        }

        public float robotSpeed;

        #region route
        public bool loopMode;
        public bool grabEveryWaypoint;
        public int currentPointIndex = 0;
        public Route[] route;
        public bool routeForward = true;
#endregion route

        private Vector3 direction;
        public Vector3 Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                if (value != Vector3.zero) transform.rotation = Quaternion.LookRotation(value);
            }
        }
        private Vector3 lastPosition;

        private Rigidbody rb;
        private Grabber grabber;
        public LayerMask obstacleLayer;

        public float maxFloorAngleDegrees;
        public Transform groundCheckOrigin;

        private bool isGrounded;
        private Vector3 groundNormal = Vector3.up;
        private float groundCheckRadius;
        private float groundCheckDistance;
        private CapsuleCollider coll;


        public float gravity = 10;

        void Start()
        {
            
            rb = GetComponent<Rigidbody>();
            grabber = GetComponentInChildren<Grabber>();

            coll = GetComponent<CapsuleCollider>();
            var radius = coll.radius;
            groundCheckRadius = radius * 0.99f; // 0.99 - padding to make spherecast detect floor
            groundCheckDistance = radius;

            CanMove = true;
            Direction = Vector3.zero;
        }

        void FixedUpdate()
        {
            lastPosition = transform.position;

            if (CanMove)
            {
                ProcessMovement();
            }
        }

        private void ProcessMovement()
        {
            if (checkIfArrived(route[currentPointIndex].waypoint))
            {
                if (grabber != null)    
                {
                    CheckGrabber();
                }
                
                if ((route != null) && (route.Length != 0))
                {
                    // switch the direction after the route has ended
                    // increase route index
                    CheckRoute();
                }

                Direction = Vector3.zero;
            }
            
            GroundCheck();
            setVelocity();
            
        }

        private void setVelocity()
        {
            Vector3 directionToPoint = GetDirectionToPoint(lastPosition, route[currentPointIndex].waypoint);
            Vector3 velocity = directionToPoint * robotSpeed;


            Vector3 velocity = rb.velocity;
            var targetVelocity = directionToPoint * robotSpeed + new Vector3(0, velocity.y, 0);
            Vector3 velocityDelta = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime) - rb.velocity;
            velocityDelta -= gravity * Time.fixedDeltaTime * groundNormal;

            rb.AddForce(velocityDelta, ForceMode.VelocityChange);
            rb.velocity = velocity;
        }

        Vector3 GetDirectionToPoint(Vector3 pointFrom, Vector3 pointTo)
        {
            Vector3 directionToPoint = pointTo - pointFrom;
            directionToPoint = new Vector3(directionToPoint.x, 0, directionToPoint.z).normalized;

            return directionToPoint;
        }

        private void GroundCheck()
        {
            var origin = groundCheckOrigin.position;

            Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out var hitInfo, groundCheckDistance);
            if (hitInfo.collider != null && rb.velocity.y < 0.01)
            {
                groundNormal = hitInfo.normal;
                var floorAngleDegrees = Mathf.Acos(Vector3.Dot(Vector3.up, groundNormal)) * Mathf.Rad2Deg;
                isGrounded = floorAngleDegrees <= maxFloorAngleDegrees;
            }
            else
            {
                isGrounded = false;
            }

            if (!isGrounded) groundNormal = Vector3.up;
        }

        private void CheckRoute()
        {
            if (routeForward == true)
            {
                if (currentPointIndex == route.Length - 1) 
                {
                    if (loopMode) currentPointIndex = -1;
                    else routeForward = false;
                }
            } 
            else if (currentPointIndex == 0) routeForward = true;

            int add_amount = routeForward ? 1 : -1;
            currentPointIndex += add_amount;
        }

        private void CheckGrabber()
        {
            rb.velocity = Vector3.zero;
            if ((grabber.grabbedItem != null) && (route[currentPointIndex].release))
            {
                grabber.ReleaseItem();
            }
            else if ((grabber.grabbedItem == null) && (route[currentPointIndex].grab))
            {
                grabber.GrabItem();
            }
        }
        private bool checkIfArrived(Vector3 point)
        {
            if (Vector3.Distance(rb.position, point) <= 1f)
            {
                Debug.Log("Robot has arrived at point.");
                return true;
            }
            else return false;
        }


        #region collision events
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == ("Player"))
            {
                Debug.Log("Collision with Player detected.");
                
                //CanMove = false;
            }
            else if (collision.gameObject.CompareTag("Item"))
            {
                Debug.Log("Collision with Item detected.");
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("Collision with object with tag Player ended.");
                
                CanMove = true;
            }
        }
        #endregion collision events

        #region route
        public bool AddWaypoint(Vector3 waypoint)
        {
            int lastElement = route.Length;
            route[lastElement].waypoint = waypoint;

            return true;
        }

        public bool AddWaypointAtIndex(Vector3 waypoint, int index)
        {
            if (index >= 0 && index < route.Length)
            {
                for (int i = index; i <= route.Length; i++)
                {
                    route[i + 1].waypoint = route[i].waypoint;
                }
                route[index].waypoint = waypoint;

                return true;
            }
            else return false;
        }
        #endregion route
    }
}
